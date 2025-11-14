using System.Collections.Generic;
using UnityEngine;

public class Glitcheable : MonoBehaviour, IInteractable
{
    public GameObject[] Barandas;
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
    [SerializeField] public List<GameObject> _objectHolograms;
    [HideInInspector] public AudioSource _audioSource;

    [Header("Visual")]
    [SerializeField] public GlitchSounds _sounds;
    [SerializeField] public float _radialDonutPS = -4.91f;

    [Header("Estados iniciales")]
    [SerializeField] public bool _startInIdle = false;

    [HideInInspector] public TimerController _timer;
    [HideInInspector] public int _index = 0;

    public Renderer _feedbackRenderer;

    public GlitchStateMachine _sm;
    public GlitchIdleState _idle;
    public GlitchDisintegratingState _dis;
    public GlitchMovingState _mov;
    public GlitchReintegratingState _rei;

    public Vector3 CurrentTargetPos => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index].position : transform.position;
    public Quaternion CurrentTargetRot => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index].rotation : transform.rotation;

    public bool IsCorrupted { get { return _sm.Current != _idle; } }

    private void Awake()
    {
        if (_newPosList.Count > 0)
        {
            for(int i=0; i < _newPosList.Count; i++)
            {
          
                _objectHolograms.Add( _newPosList[i].gameObject);
                
            }
        }
        if (_coll == null)
            _coll = GetComponent<Collider>();
        
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        
        _audioSource = GetComponent<AudioSource>();
        _ps.Stop();
        _timer = GetComponent<TimerController>();

        SetAlpha(1f);
        SetFeedbackAlpha(0f);
        SetBoolCorrupted(0f);
        SetParticles(false, 1f);
        SetColliders(true);

        _sm = new GlitchStateMachine();

        _idle = new GlitchIdleState(this);
        _dis = new GlitchDisintegratingState(this);
        _mov = new GlitchMovingState(this);
        _rei = new GlitchReintegratingState(this);

        _idle.SetNext(_dis);
        _dis.SetNext(_mov, _idle);
        _mov.SetNext(_rei);
        _rei.SetNext(_dis, _idle);

        _sm.Change(_startInIdle ? _idle : _dis);
    }

    private void Update()
    {
        _sm.Tick(Time.deltaTime);
    }
    public void HologramSwitch()
    {
        if (_objectHolograms.Count > 0)
        {
            for(int i=0; i < _objectHolograms.Count; i++)
            {
                _objectHolograms[i].SetActive(!_objectHolograms[i].activeSelf);
            }
        }
    }
    public void BeginCycle()
    {
        _sm.Change(_dis.ResetAndReturn());
    }

    private bool CheckStateChange(NodeType nodeType)
    {
        if (nodeType == NodeType.None)
            return false;

        bool toIdleCase = _sm.Current != _idle && nodeType == NodeType.Default;
        bool toGlitchedCase = _sm.Current == _idle && nodeType == NodeType.Corrupted;

        return toIdleCase || toGlitchedCase;
    }

    public bool CanInteract(PlayerNodeHandler player)
    {
        if (!CheckStateChange(player.CurrentType) || _sm.Current is not IGlitchInterruptible ii) return false;

        return true;
    }

    public void Interact(PlayerNodeHandler player, out bool succeededInteraction)
    {
        succeededInteraction = CanInteract(player);
        
        if (succeededInteraction)
        {
            IGlitchInterruptible ii = (IGlitchInterruptible) _sm.Current;
            ii.Interrupt();
        }
    }

    public void SetAlpha(float a)
    {
        _renderer.material.SetFloat("_Alpha", Mathf.Clamp01(a));
    }

    public void SetFeedbackAlpha(float a)
    {
        _feedbackRenderer.material.SetFloat("_Alpha", Mathf.Clamp01(a));
    }

    public void SetBoolCorrupted(float v)
    {
        _renderer.material.SetFloat("_IsCorrupted", v);
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
}