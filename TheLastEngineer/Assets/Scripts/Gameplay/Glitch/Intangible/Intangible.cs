using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Object the player can make passable: holds only Clean or Intangible (GlitchSwapRules). In the
/// Intangible state it moves to _intangibleLayer and dissolves (_Alpha 1 -> 0, VFX particles on); the player
/// passes through only while carrying an Intangible node, because PlayerIntangibilityHandler excludes that layer.
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
    [Tooltip("Particles shown while Intangible. Enabling the component restarts it, so its initial event (OnPlay) fires the burst again.")]
    [SerializeField] private VisualEffect _vfx;

    [Header("Layers")]
    [SerializeField] private LayerMask _defaultLayer;
    [Tooltip("Layer while Intangible. The player excludes it (PlayerData.intangibleExcludeLayers) while carrying an Intangible node.")]
    [SerializeField] private LayerMask _intangibleLayer;

    [Header("Visual")]
    [Tooltip("Duration of the _Alpha fade when the state changes.")]
    [SerializeField] private float _fadeTime = 0.4f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private const float SolidAlpha = 1f;
    private const float PhasedAlpha = 0f;
    private const float OverlapSkin = 0.02f;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly Collider[] _overlapBuffer = new Collider[16];

    private GlitchComponent _glitch;
    private Material _material;
    private Tween _fadeTween;
    private bool _animateVisual;
    private bool _visualStarted;
    private bool _visuallyPhased;

    private GlitchStateMachine _fsm;
    private IntangibleCleanState _cleanState;
    private IntangiblePhasedState _phasedState;
    private IntangibleExitingState _exitingState;

    public GlitchComponent Glitch => _glitch;
    public bool IsPhased => _glitch != null && _glitch.CurrentState == GlitchState.Intangible;

    /// <summary>
    /// What the visual shows, which can lag IsPhased: back to Clean with the player inside (Exiting) it stays
    /// phased until they leave. Before Start it falls back to IsPhased, so listeners can read it in their own Start.
    /// </summary>
    public bool IsVisuallyPhased => _visualStarted ? _visuallyPhased : IsPhased;

    public System.Action OnInteractionRejected;

    /// <summary>Raised when the visual starts changing (true = phased). Listened to by IntangibleGhost.</summary>
    public event System.Action<bool> OnPhaseVisualChanged;

    private void Awake()
    {
        if (_coll == null) _coll = GetComponent<Collider>();
        if (_renderer == null) _renderer = GetComponent<Renderer>();

        ResolveMaterial();

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
        _visualStarted = true;

        _glitch.OnGlitchStateChanged += OnGlitchStateChanged;
    }

#if UNITY_EDITOR
    private void Reset() => GlitchContainerExclusivity.RejectIfPresent<Glitcheable>(this);
#endif

    private void OnDestroy()
    {
        if (_glitch != null) _glitch.OnGlitchStateChanged -= OnGlitchStateChanged;
        if (_fadeTween.isAlive) _fadeTween.Stop();

        // renderer.material creates a copy Unity doesn't release by itself.
        if (_material != null) Destroy(_material);
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
    /// Phased: _Alpha to 0 and the particles start right away. Solid: _Alpha to 1 and the particles are cleared
    /// once the fade ends (a fade interrupted mid-way keeps them). Interpolates from the current value so a
    /// mid-fade change doesn't jump.
    /// </summary>
    public void FadeVisual(bool phased)
    {
        if (_fadeTween.isAlive) _fadeTween.Stop();

        _visuallyPhased = phased;
        float target = phased ? PhasedAlpha : SolidAlpha;

        if (phased) SetParticles(true);

        if (!_animateVisual || _fadeTime <= 0f)
        {
            SetAlpha(target);
            if (!phased) SetParticles(false);
        }
        else
        {
            float from = _material != null ? _material.GetFloat(AlphaId) : target;
            _fadeTween = Tween.Custom(this, from, target, _fadeTime, (owner, a) => owner.SetAlpha(a), _ease);

            // TODO: fade the particles out inside VFXGraph_RepelObject so they die on their own instead of being cleared here.
            if (!phased) _fadeTween.OnComplete(this, owner => owner.SetParticles(false), warnIfTargetDestroyed: false);
        }

        OnPhaseVisualChanged?.Invoke(phased);
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

    /// <summary>Own copy (renderer.material): writing the shared .mat would change every object.</summary>
    private void ResolveMaterial()
    {
        if (_renderer == null)
        {
            Debug.LogWarning($"[Intangible] {name}: no Renderer, the state change has no visual", this);
            return;
        }

        _material = _renderer.material;
        if (!_material.HasProperty(AlphaId))
            Debug.LogWarning($"[Intangible] {name}: material {_material.name} has no _Alpha", this);
    }

    private void SetAlpha(float alpha)
    {
        if (_material != null) _material.SetFloat(AlphaId, alpha);
    }

    // Enabling a VisualEffect restarts it and fires its burst again; disabling clears every particle.
    private void SetParticles(bool on)
    {
        if (_vfx != null && _vfx.enabled != on) _vfx.enabled = on;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
