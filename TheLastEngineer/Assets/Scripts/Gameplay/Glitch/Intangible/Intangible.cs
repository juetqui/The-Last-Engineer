using PrimeTween;
using UnityEngine;

/// <summary>
/// Object the player can make passable: holds only Clean or Intangible (GlitchSwapRules). In the
/// Intangible state it moves to _intangibleLayer and shows the overlay; the player passes through
/// only while carrying an Intangible node, because PlayerIntangibilityHandler excludes that layer.
/// The collider is never disabled, so the object stays registered and Take can always reach it.
/// Mutually exclusive with Glitcheable on a GameObject.
/// </summary>
public class Intangible : MonoBehaviour, IGlitchSwapTarget
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    public GlitchContainerKind Kind => GlitchContainerKind.Intangible;
    #endregion

    [Header("Refs")]
    [SerializeField] private Collider _coll;
    [SerializeField] private Renderer _renderer;

    [Header("Layers")]
    [SerializeField] private LayerMask _defaultLayer;
    [Tooltip("Layer while Intangible. The player excludes it (PlayerData.intangibleExcludeLayers) while carrying an Intangible node.")]
    [SerializeField] private LayerMask _intangibleLayer;

    [Header("Visual")]
    [Tooltip("Duration of the overlay and distortion fade when the state changes.")]
    [SerializeField] private float _fadeTime = 0.4f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // Renderer slots: [0] glitch material (distortion) and [1] overlay shown while Intangible.
    private const int GlitchMatIndex = 0;
    private const int OverlayMatIndex = 1;
    private const float OverlapSkin = 0.02f;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int DistortionId = Shader.PropertyToID("_DistortionAmount");
    private static readonly Collider[] _overlapBuffer = new Collider[16];

    private GlitchComponent _glitch;
    private Material[] _materialInstances;
    private Material _glitchMat;
    private Material _overlayMat;
    private float _initialDistortion;
    private Tween _fadeTween;
    private bool _animateVisual;

    private GlitchStateMachine _fsm;
    private IntangibleCleanState _cleanState;
    private IntangiblePhasedState _phasedState;
    private IntangibleExitingState _exitingState;

    public GlitchComponent Glitch => _glitch;
    public bool IsPhased => _glitch != null && _glitch.CurrentState == GlitchState.Intangible;

    public System.Action OnInteractionRejected;

    private void Awake()
    {
        if (_coll == null) _coll = GetComponent<Collider>();
        if (_renderer == null) _renderer = GetComponent<Renderer>();

        ResolveMaterials();

        _glitch = GlitchComponent.Ensure(gameObject);
        _glitch.SanitizeOnLoad(Kind);

        _fsm = new GlitchStateMachine();
        _cleanState = new IntangibleCleanState(this);
        _phasedState = new IntangiblePhasedState(this);
        _exitingState = new IntangibleExitingState(this);
    }

    private void Start()
    {
        // Initial look without animating, read by pull: GlitchComponent never raises its initial state.
        _animateVisual = false;
        _fsm.Change(IsPhased ? _phasedState : _cleanState);
        _animateVisual = true;

        _glitch.OnGlitchStateChanged += OnGlitchStateChanged;
    }

#if UNITY_EDITOR
    private void Reset() => GlitchContainerExclusivity.RejectIfPresent<Glitcheable>(this);
#endif

    private void OnDestroy()
    {
        if (_glitch != null) _glitch.OnGlitchStateChanged -= OnGlitchStateChanged;
        if (_fadeTween.isAlive) _fadeTween.Stop();

        // renderer.materials creates copies Unity doesn't release by itself.
        if (_materialInstances == null) return;

        foreach (var mat in _materialInstances)
            if (mat != null) Destroy(mat);
    }

    private void Update() => _fsm.Tick(Time.deltaTime);

    private void OnGlitchStateChanged(GlitchState state)
    {
        if (state == GlitchState.Intangible) _fsm.Change(_phasedState);
        else _fsm.Change(IsPlayerInside() ? _exitingState : _cleanState);
    }

    #region -----STATE API-----
    public void EnterClean() => _fsm.Change(_cleanState);

    public void ApplyLayer(bool phased)
    {
        var mask = (phased ? _intangibleLayer : _defaultLayer).value;
        if (mask == 0)
        {
            Debug.LogWarning($"[Intangible] {name}: {(phased ? "_intangibleLayer" : "_defaultLayer")} is empty", this);
            return;
        }

        int layerIndex = Mathf.RoundToInt(Mathf.Log(mask, 2));
        if (layerIndex == gameObject.layer) return;

        SetLayerRecursively(gameObject, layerIndex);

        // A layer change can break the pair with the player detector's trigger: re-announce.
        if (PlayerController.Instance != null)
            PlayerController.Instance.RescanInteractable(this, _coll);

        if (debug) Debug.Log($"[Intangible] {name} -> layer {layerIndex}", this);
    }

    /// <summary>
    /// Overlay (_Alpha) to 1 and distortion to 0 while phased; the opposite otherwise (distortion
    /// back to its initial value). Interpolates from the current values so a mid-fade change doesn't jump.
    /// </summary>
    public void FadeVisual(bool phased)
    {
        if (_fadeTween.isAlive) _fadeTween.Stop();

        float targetAlpha = phased ? 1f : 0f;
        float targetDistortion = phased ? 0f : _initialDistortion;

        if (!_animateVisual || _fadeTime <= 0f)
        {
            SetVisual(targetAlpha, targetDistortion);
            return;
        }

        float fromAlpha = _overlayMat != null ? _overlayMat.GetFloat(AlphaId) : targetAlpha;
        float fromDistortion = _glitchMat != null ? _glitchMat.GetFloat(DistortionId) : targetDistortion;

        _fadeTween = Tween.Custom(gameObject, 0f, 1f, _fadeTime, (_, t) =>
            SetVisual(Mathf.Lerp(fromAlpha, targetAlpha, t), Mathf.Lerp(fromDistortion, targetDistortion, t)),
            _ease);
    }

    /// <summary>Leaving the intangible layer with the player inside would trap them: Exiting waits on this.</summary>
    public bool IsPlayerInside()
    {
        var player = PlayerController.Instance;
        if (player == null || player.CC == null || _coll == null || !_coll.enabled) return false;

        var cc = player.CC;
        var t = cc.transform;
        var scale = t.lossyScale;

        var radius = cc.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        var height = Mathf.Max(cc.height * Mathf.Abs(scale.y), radius * 2f);
        var center = t.TransformPoint(cc.center);
        var offset = t.up * (height * 0.5f - radius);

        // The skin keeps leaning against a face from counting as inside.
        var count = Physics.OverlapCapsuleNonAlloc(center + offset, center - offset, Mathf.Max(0.01f, radius - OverlapSkin),
            _overlapBuffer, 1 << _coll.gameObject.layer, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
            if (_overlapBuffer[i] == _coll) return true;

        return false;
    }
    #endregion

    // Interact is only for physical manipulation; state moves with Set/Take. Being IInteractable
    // keeps us registered in PlayerInteractionDetector, and false keeps us out of Interact selection.
    public bool CanInteract(PlayerNodeHandler player) => false;

    public void Interact(PlayerNodeHandler player, out bool succeededInteraction)
    {
        succeededInteraction = false;
        NotifyRejected();
    }

    public void NotifyRejected() => OnInteractionRejected?.Invoke();

    /// <summary>Own copies (renderer.materials): writing the shared .mat would change every object.</summary>
    private void ResolveMaterials()
    {
        if (_renderer == null)
        {
            Debug.LogWarning($"[Intangible] {name}: no Renderer, the state change has no visual", this);
            return;
        }

        _materialInstances = _renderer.materials;
        _glitchMat = _materialInstances.Length > GlitchMatIndex ? _materialInstances[GlitchMatIndex] : null;
        _overlayMat = _materialInstances.Length > OverlayMatIndex ? _materialInstances[OverlayMatIndex] : null;

        if (_overlayMat == null)
            Debug.LogWarning($"[Intangible] {name}: renderer has no overlay in slot {OverlayMatIndex}", this);

        // Read before anything changes it: the value distortion returns to when leaving Intangible.
        _initialDistortion = _glitchMat != null && _glitchMat.HasProperty(DistortionId) ? _glitchMat.GetFloat(DistortionId) : 0f;
    }

    private void SetVisual(float alpha, float distortion)
    {
        if (_overlayMat != null) _overlayMat.SetFloat(AlphaId, alpha);
        if (_glitchMat != null && _glitchMat.HasProperty(DistortionId)) _glitchMat.SetFloat(DistortionId, distortion);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
