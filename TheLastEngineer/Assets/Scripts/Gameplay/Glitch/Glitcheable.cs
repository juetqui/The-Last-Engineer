using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Glitcheable : MonoBehaviour, IInteractable
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
    [SerializeField] public bool _startInIdle = false;
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
        if (_startInIdle && _isPlatform) _index++; 
        
        FSM.Change(_startInIdle ? IdleState : DisState);
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

    private bool CheckStateChange(NodeType nodeType)
    {
        if (nodeType == NodeType.None)
            return false;

        var toIdleCase = FSM.Current != IdleState && nodeType == NodeType.Default;
        var toGlitchedCase = FSM.Current == IdleState && nodeType == NodeType.Corrupted;

        return toIdleCase || toGlitchedCase;
    }

    public bool CanInteract(PlayerNodeHandler player)
    {
        var canInteract = CheckStateChange(player.CurrentType) && FSM.Current is IGlitchInterruptible;

        if (!canInteract) OnInteractionRejected?.Invoke();

        return canInteract;
    }

    public void Interact(PlayerNodeHandler player, out bool succeededInteraction)
    {
        succeededInteraction = CanInteract(player);

        if (!succeededInteraction) return;

        var ii = (IGlitchInterruptible) FSM.Current;
        ii.Interrupt();
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
    }

    public void AdvanceToNextNode()
    {
        if (_newPosList == null || _newPosList.Count == 0) return;
        _index = (_index + 1) % _newPosList.Count;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            OnPlayerInRange?.Invoke(player, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            OnPlayerInRange?.Invoke(player, false);
    }
}