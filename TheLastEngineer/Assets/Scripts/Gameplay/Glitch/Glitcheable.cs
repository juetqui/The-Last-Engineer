using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using Unity.VisualScripting;

public class Glitcheable : MonoBehaviour
{
    [Header("Refs")]
    [DoNotSerialize] public Collider _coll;
    [DoNotSerialize] public Renderer _renderer;
    [SerializeField] public ParticleSystem _ps;
    [SerializeField] public List<Transform> _newPosList;
    [DoNotSerialize] public AudioSource _audioSource;

    [Header("Visual")]
    [SerializeField] public GlitchSounds _sounds;
    [SerializeField] public float _radialDonutPS = -4.91f;

    [Header("Estados iniciales")]
    [SerializeField] public bool _startInIdle = false;

    [DoNotSerialize] public TimerController _timer;
    [DoNotSerialize] public DecalProjector _decalProjector;
    public int _index = 0;

    public Renderer _feedbackRenderer;

    public GlitchStateMachine _sm;
    public GlitchIdleState _idle;
    public GlitchDisintegratingState _dis;
    public GlitchMovingState _mov;
    public GlitchReintegratingState _rei;

    public Vector3 CurrentTargetPos => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index].position : transform.position;
    public Quaternion CurrentTargetRot => _newPosList != null && _newPosList.Count > 0 ? _newPosList[_index].rotation : transform.rotation;

    public bool IsCorrupted { get { return _startInIdle; } }

    private void Awake()
    {
        _coll = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
        _audioSource = GetComponent<AudioSource>();
        _ps.Stop();
        _timer = GetComponent<TimerController>();
        _decalProjector = GetComponentInChildren<DecalProjector>();

        SetAlpha(1f);
        SetFeedbackAlpha(0f);
        SetDecal(0f);
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

    public void BeginCycle()
    {
        _sm.Change(_dis.ResetAndReturn());
    }

    public bool Interrupt()
    {
        if (_sm.Current is IGlitchInterruptible ii)
        {
            ii.Interrupt();
            return true;
        }
        else return false;
    }

    public void SetAlpha(float a)
    {
        _renderer.material.SetFloat("_Alpha", Mathf.Clamp01(a));
    }

    public void SetFeedbackAlpha(float a)
    {
        _feedbackRenderer.material.SetFloat("_Alpha", Mathf.Clamp01(a));
    }

    public void SetDecal(float v)
    {
        _decalProjector.material.SetFloat("_CorrruptedControl", v);
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