using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Glitcheable : MonoBehaviour, IInteractable, IProximityListener
{
    public GameObject[] handrails;
    
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
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
    // El viejo _startInIdle se fue: ahora el estado inicial lo decide el nivel del GlitchComponent
    // (Glitched arranca el ciclo, Clean e Intangible se quedan en Idle).
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

        if (_feedbackRenderer != null)
            _feedbackPS = _feedbackRenderer.GetComponentInChildren<ParticleSystem>().emission;
        
        _audioSource = GetComponent<AudioSource>();
        _ps.Stop();
        _timer = GetComponent<TimerController>();

        // Se puede leer con seguridad desde acá aunque esta clase corra con DefaultExecutionOrder(-1):
        // GlitchComponent no inicializa nada en Awake, su nivel es el campo serializado.
        _glitch = GlitchComponent.Ensure(gameObject);

        SetAlpha(1f);
        SetFeedbackAlpha(0f);
        // SetBoolCorrupted(0f);
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

        // Las suscripciones van DESPUÉS del Change inicial: así el arranque no pasa por el
        // reconciliador. Igual es idempotente, pero es más fácil de seguir en el debugger.
        _glitch.OnGlitchStateChanged += OnLevelChanged;
        FSM.OnStateChanged += OnFsmStateChanged;
    }

    private void OnDestroy()
    {
        if (_glitch != null) _glitch.OnGlitchStateChanged -= OnLevelChanged;
        if (FSM != null) FSM.OnStateChanged -= OnFsmStateChanged;
    }

    private void OnLevelChanged(GlitchState level) => SyncFsmWithLevel();
    private void OnFsmStateChanged(IState state) => SyncFsmWithLevel();

    /// <summary>
    /// Acerca la FSM a lo que pide el nivel. Es idempotente y se llama desde los dos lados (cambio
    /// de nivel y cambio de estado) porque GlitchMovingState no es interrumpible: si el jugador
    /// descarga el objeto mientras se está moviendo, no se puede cortar ahí sin partir el lerp y
    /// el reparent de GlitchMovingState.Exit. Se deja terminar el movimiento y este mismo método
    /// vuelve a correr al entrar a Reintegrating, que sí es interrumpible.
    /// </summary>
    private void SyncFsmWithLevel()
    {
        // La FSM emite OnStateChanged DESPUÉS del Enter del estado nuevo, así que un Change hecho
        // desde acá reentraría en este mismo método.
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
    public void HologramSwitch(bool enable)
    {
        if (_objectHolograms.Count <= 0) return;

        var closest = _objectHolograms.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
        closest.enabled = enable;
    }

    public void BeginCycle()
    {
        FSM.Change(DisState.ResetAndReturn());
    }

    // Interact quedó exclusivo de manipulación física (agarrar, soltar, enchufar); el glitch se
    // mueve con Set/Take. Seguimos siendo IInteractable solo para que PlayerInteractionDetector
    // nos siga registrando: GetClosestGlitcheable sale de esa misma lista y, a diferencia de
    // GetInteractable, NO filtra por CanInteract.
    //
    // Devolver false además saca al Glitcheable de la selección por prioridad: aunque seguimos
    // declarando Highest, GetInteractable filtra por CanInteract antes de ordenar, así que dejamos
    // de tapar al NodeController o a la Connection que el jugador quiere usar con Interact.
    public bool CanInteract(PlayerNodeHandler player) => false;

    public void Interact(PlayerNodeHandler player, out bool succeededInteraction)
    {
        succeededInteraction = false;
        OnInteractionRejected?.Invoke();
    }

    public void SetAlpha(float a)
    {
        _renderer.material.SetFloat("_Alpha", Mathf.Clamp01(a));
    }

    public void SetFeedbackAlpha(float a)
    {
        var clampedAlpha = Mathf.Clamp01(a);

        _feedbackPS.rateOverTime = Mathf.Lerp(feedbackMinMaxPS.x, feedbackMinMaxPS.y, clampedAlpha);
    }

    public void SetBoolCorrupted(float v)
    {
        _renderer.material.SetFloat("_IsCorrupted", v);

        var targetLayer = _glitchedLayer;
        if (FSM.Current != IdleState) targetLayer = _defaultLayer;

        int mask = targetLayer.value;
        if (mask == 0) { if (debug) Debug.LogWarning($"[Glitcheable] {name}: LayerMask vacía", this); return; }                                                                                                                                                                                                              
        int layerIndex = Mathf.RoundToInt(Mathf.Log(mask, 2));
        SetLayerRecursively(gameObject, layerIndex);

        if (debug) Debug.Log($"[Glitcheable] {name} -> layer {layerIndex} (corrupted={v}, state={FSM.Current?.GetType().Name})", this);
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
        var main = _ps.main;

        if (on) _ps.Play();
        else _ps.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    public void SetColliders(bool enable)
    {
        _coll.enabled = enable;

        // Mismo motivo que en NodeController.Attach: el collider se apaga y se vuelve a prender
        // (acá además el objeto se mueve y cambia de layer en el medio), y ninguno de esos casos
        // garantiza que PhysX emita OnTriggerEnter/Exit. Nos re-anunciamos al detector del jugador.
        if (PlayerController.Instance != null)
            PlayerController.Instance.RescanInteractable(this, _coll);
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