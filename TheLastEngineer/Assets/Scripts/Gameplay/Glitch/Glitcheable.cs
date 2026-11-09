using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Object whose Glitched state drives a cycle: disintegrate, move to the next anchor, reintegrate.
/// Holds only Clean or Glitched (GlitchSwapRules); passable objects are Intangible, never both.
/// </summary>
[DefaultExecutionOrder(-1)]
public class Glitcheable : MonoBehaviour, IGlitchSwapTarget, IProximityListener
{
    public GameObject[] handrails;

    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    public GlitchContainerKind Kind => GlitchContainerKind.Glitcheable;
    #endregion

    [Header("Refs")]
    public Collider _coll;
    [HideInInspector] public Renderer _renderer;
    [SerializeField] public ParticleSystem _ps;
    [SerializeField] public List<Transform> _newPosList;
    [HideInInspector] public AudioSource _audioSource;

    private List<MeshRenderer> _objectHolograms;

    [Header("Visual")]
    [SerializeField] public GlitchSounds _sounds;
    [SerializeField] public float _radialDonutPS = -4.91f;

    [Header("Estados iniciales")]
    // The initial FSM state comes from the GlitchComponent: Glitched starts the cycle, Clean idles.
    [SerializeField] private bool _isPlatform = false;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    public bool IsPlatform => _isPlatform;

    [HideInInspector] public TimerController _timer;
    [HideInInspector] public int _index = 0;

    public Renderer _feedbackRenderer;
    [SerializeField] private Vector2 feedbackMinMaxPS = new Vector2(0, 200);
    [SerializeField] private LayerMask _defaultLayer;
    [SerializeField] private LayerMask _glitchedLayer;
    private ParticleSystem.EmissionModule _feedbackPS;

    // Renderer slot [0] is the glitch material (alpha, corruption).
    private const int GlitchMatIndex = 0;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int IsCorruptedId = Shader.PropertyToID("_IsCorrupted");

    private Material[] _materialInstances;
    private Material _glitchMat;

    public GlitchStateMachine FSM;
    public GlitchIdleState IdleState;
    public GlitchDisintegratingState DisState;
    public GlitchMovingState MovState;
    public GlitchReintegratingState ReiState;

    public Transform CurrentTarget => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index] : transform;

    public Vector3 CurrentTargetPos => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index].position : transform.position;
    public Quaternion CurrentTargetRot => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index].rotation : transform.rotation;

    public bool IsCorrupted { get { return FSM.Current != IdleState; } }

    private GlitchComponent _glitch = default;
    private bool _syncing = false;

    public GlitchComponent Glitch => _glitch;
    public GlitchState Level => _glitch.CurrentState;

    public Action<PlayerController, bool> OnPlayerInRange;
    public Action OnInteractionRejected;

    private void Awake()
    {
        if (_newPosList.Count > 0)
        {
            _objectHolograms = new List<MeshRenderer>();

            foreach (var anchor in _newPosList)
            {
                var meshRenderer = anchor.gameObject.GetComponent<MeshRenderer>();
                _objectHolograms.Add(meshRenderer);
            }
        }

        if (_coll == null)
            _coll = GetComponent<Collider>();

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        ResolveMaterials();

        if (_feedbackRenderer != null)
            _feedbackPS = _feedbackRenderer.GetComponentInChildren<ParticleSystem>().emission;

        _audioSource = GetComponent<AudioSource>();
        _ps.Stop();
        _timer = GetComponent<TimerController>();

        // Safe to read here despite DefaultExecutionOrder(-1): GlitchComponent has no Awake init,
        // its state is the serialized field.
        _glitch = GlitchComponent.Ensure(gameObject);
        _glitch.SanitizeOnLoad(Kind);

        SetAlpha(1f);
        SetFeedbackAlpha(0f);
        SetParticles(false, 1f);
        SetColliders(true);

        FSM = new GlitchStateMachine();

        IdleState = new GlitchIdleState(this);
        DisState = new GlitchDisintegratingState(this);
        MovState = new GlitchMovingState(this);
        ReiState = new GlitchReintegratingState(this);

        IdleState.SetNext(DisState);
        DisState.SetNext(MovState, IdleState);
        MovState.SetNext(ReiState);
        ReiState.SetNext(DisState, IdleState);
    }

    private void Start()
    {
        var startsCycling = Level == GlitchState.Glitched;

        if (!startsCycling && _isPlatform) _index++;

        FSM.Change(startsCycling ? DisState : IdleState);

        // Subscribed AFTER the initial Change so startup doesn't go through the reconciler.
        _glitch.OnGlitchStateChanged += OnLevelChanged;
        FSM.OnStateChanged += OnFsmStateChanged;
    }

#if UNITY_EDITOR
    private void Reset() => GlitchContainerExclusivity.RejectIfPresent<Intangible>(this);
#endif

    private void OnDestroy()
    {
        if (_glitch != null) _glitch.OnGlitchStateChanged -= OnLevelChanged;
        if (FSM != null) FSM.OnStateChanged -= OnFsmStateChanged;

        // renderer.materials creates copies Unity doesn't release by itself.
        if (_materialInstances != null)
        {
            foreach (var mat in _materialInstances)
                if (mat != null) Destroy(mat);
        }
    }

    /// <summary>
    /// Takes this object's own copies (renderer.materials, not sharedMaterials): writing to the
    /// shared .mat would change every glitcheable and, in the Editor, persist after Play.
    /// </summary>
    private void ResolveMaterials()
    {
        _materialInstances = _renderer.materials;
        _glitchMat = _materialInstances.Length > GlitchMatIndex ? _materialInstances[GlitchMatIndex] : null;

        if (_glitchMat == null)
            Debug.LogWarning($"[Glitcheable] {name}: renderer has no material in slot {GlitchMatIndex}", this);
    }

    private void OnLevelChanged(GlitchState level) => SyncFsmWithLevel();

    private void OnFsmStateChanged(IState state) => SyncFsmWithLevel();

    /// <summary>
    /// Moves the FSM towards what the state asks for. Idempotent and called from both sides (state
    /// change and FSM change) because GlitchMovingState is not interruptible: if the player takes
    /// the charge while it moves, the move finishes and this runs again on entering Reintegrating,
    /// which is interruptible.
    /// </summary>
    private void SyncFsmWithLevel()
    {
        // The FSM raises OnStateChanged AFTER the new state's Enter, so a Change made from here
        // would re-enter this method.
        if (_syncing) return;

        _syncing = true;

        var wantsCycle = Level == GlitchState.Glitched;

        if (wantsCycle && FSM.Current == IdleState)
            BeginCycle();
        else if (!wantsCycle && FSM.Current != IdleState && FSM.Current is IGlitchInterruptible interruptible)
            interruptible.Interrupt();

        _syncing = false;
    }

    private void Update()
    {
        FSM.Tick(Time.deltaTime);
    }

    private void Rescan()
    {
        // The collider is switched off and on (and the object moves and changes layer in between),
        // and PhysX doesn't guarantee OnTriggerEnter/Exit for any of it: re-announce ourselves.
        if (PlayerController.Instance != null)
            PlayerController.Instance.RescanInteractable(this, _coll);
    }

    public void HologramSwitch(bool enable)
    {
        if (_objectHolograms == null || _objectHolograms.Count <= 0) return;

        var closest = _objectHolograms.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
        closest.enabled = enable;
    }

    public void BeginCycle()
    {
        FSM.Change(DisState.ResetAndReturn());
    }

    // Interact is only for physical manipulation; glitch moves with Set/Take. We stay IInteractable
    // so PlayerInteractionDetector keeps registering us, and returning false keeps us out of the
    // Interact priority selection.
    public bool CanInteract(PlayerNodeHandler player) => false;

    public void Interact(PlayerNodeHandler player, out bool succeededInteraction)
    {
        succeededInteraction = false;
        NotifyRejected();
    }

    public void NotifyRejected() => OnInteractionRejected?.Invoke();

    public void SetAlpha(float a)
    {
        if (_glitchMat != null) _glitchMat.SetFloat(AlphaId, Mathf.Clamp01(a));
    }

    public float GetAlpha() => _glitchMat != null ? Mathf.Clamp01(_glitchMat.GetFloat(AlphaId)) : 0f;

    public void SetFeedbackAlpha(float a)
    {
        var clampedAlpha = Mathf.Clamp01(a);

        _feedbackPS.rateOverTime = Mathf.Lerp(feedbackMinMaxPS.x, feedbackMinMaxPS.y, clampedAlpha);
    }

    public void SetBoolCorrupted(float v)
    {
        if (_glitchMat != null) _glitchMat.SetFloat(IsCorruptedId, v);

        ApplyLayer();
    }

    /// <summary>Glitched layer while the cycle runs, default layer while idle.</summary>
    private void ApplyLayer()
    {
        int mask = (FSM.Current != IdleState ? _glitchedLayer : _defaultLayer).value;
        if (mask == 0) { if (debug) Debug.LogWarning($"[Glitcheable] {name}: empty LayerMask", this); return; }

        int layerIndex = Mathf.RoundToInt(Mathf.Log(mask, 2));
        bool changed = layerIndex != gameObject.layer;

        SetLayerRecursively(gameObject, layerIndex);

        // A layer change can break the pair with the detector's trigger.
        if (changed) Rescan();

        if (debug) Debug.Log($"[Glitcheable] {name} -> layer {layerIndex} (level={Level}, state={FSM.Current?.GetType().Name})", this);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public void SetParticles(bool on, float radial)
    {
        var vel = _ps.velocityOverLifetime;
        vel.radial = radial;

        if (on) _ps.Play();
        else _ps.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    // Called by the FSM states.
    public void SetColliders(bool enable)
    {
        _coll.enabled = enable;
        Rescan();
    }

    public void AdvanceToNextNode()
    {
        if (_newPosList == null || _newPosList.Count == 0) return;
        _index = (_index + 1) % _newPosList.Count;
    }

    public void OnPlayerProximity(bool inRange, PlayerController player)
    {
        OnPlayerInRange?.Invoke(player, inRange);
    }
}
