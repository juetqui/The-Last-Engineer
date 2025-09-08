using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class Glitcheable : MonoBehaviour
{
    [SerializeField] private GlitchSounds _sounds;
    [SerializeField] private Collider _coll;
    [SerializeField] private Collider _triggerColl;
    [SerializeField] private Renderer _renderer;
    [SerializeField] protected ParticleSystem _ps;
    [SerializeField] private Transform _feedbackPos;
    [SerializeField] protected List<Transform> _newPosList;
    [SerializeField] protected bool _isPlatform = false;
    [SerializeField] protected float _radialDonutPS = -4.91f;
    [SerializeField] protected bool _isCorrupted = true;
    [SerializeField] PlatformWalls[] platformWalls;
    [SerializeField] GameObject _feedBackPlane;
    
    protected TimerController _timerController;
    public DecalProjector decalProjector;
    protected List<Transform> _currentList = default;
    protected bool _canMove = true;
    protected bool _isStopped = false;
    private bool _isIntargeteable = false;
    protected int _index = 0;
    private PlayerController _player = null;
    private Renderer _feedbackRenderer = default;
    private Coroutine _coroutine = null;
    private AudioSource _audioSource = default;
    private NodeType _requiredNode = NodeType.Corrupted;
    protected Vector3 _targetPos = default;
    private Quaternion _targetRot = default;

    public bool IsIntargeteable { get { return _isIntargeteable; } }
    public bool IsCorrupted { get { return _isCorrupted; } }
    public bool IsStopped { get { return _isStopped; } }

    public Action<Vector3> OnPosChanged = delegate { };

    public int GetOnPosChangedHandlerCount()
    {
        return OnPosChanged?.GetInvocationList().Length ?? 0;
    }

    protected void OnAwake()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();

        _timerController = GetComponent<TimerController>();
        _audioSource = GetComponent<AudioSource>();
        _feedbackRenderer = _feedbackPos.GetComponent<Renderer>();

        _currentList = _newPosList;
        _targetPos = _currentList[_index].position;
        _targetRot = _currentList[_index].rotation;

        decalProjector = GetComponentInChildren<DecalProjector>();

        _ps.Stop();
    }
   

    public void CheckTimerPhase(Phase currentPhase)
    {
        if (_isStopped || !_isCorrupted) return;

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        if (currentPhase == Phase.Transparency)
        {
            _coroutine = StartCoroutine(SetTransparency());
        }
        else if (currentPhase == Phase.Movement)
        {
            _coroutine = StartCoroutine(MoveTrail());
        }
        else if (currentPhase == Phase.ReverseTransparency)
        {
            _coroutine = StartCoroutine(ResetTransparency());
        }
    }

    public bool ChangeCorruptionState(NodeType nodeType, bool newState)
    {
        if (newState == _isCorrupted) return false;
        
        StopAllCoroutines();

        _isCorrupted = newState;

        if (_isCorrupted)
        {
            _timerController.ResumeCycle();
            
            if (decalProjector != null)
                decalProjector.material.SetFloat("_CorrruptedControl", 1f);
        }
        else
        {
            _timerController.StopCycle();

            _renderer.material.SetFloat("_Alpha", 1f);
            _feedbackRenderer.material.SetFloat("_Alpha", 0f);
            
            if (decalProjector != null)
                decalProjector.material.SetFloat("_CorrruptedControl", 0f);

            var ps = _ps.main;
            var psVel = _ps.velocityOverLifetime;
            psVel.radial = 1f;
            ps.loop = true;
            _ps.Play();
        }

        return true;
    }

    protected void UpdateTarget()
    {
        if (_isStopped || !_isCorrupted) return;

        if (_index == _currentList.Count - 1) _index = 0;
        else _index++;

        transform.position = _targetPos;
        transform.rotation = _targetRot;

        _targetPos = _currentList[_index].position;
        _targetRot = _currentList[_index].rotation;
    }

    public void PositionReset()
    {
        transform.position = _currentList[_currentList.Count - 1].position;
        transform.rotation = _currentList[_currentList.Count - 1].rotation;
        _isStopped = false;
        _index = 0;
    }

    private IEnumerator SetTransparency()
    {
        _isIntargeteable = true;

        if (_player != null)
        {
            _player.SetCanMove(false);
            _player.StartDesintegratePlayer();
            OnPosChanged?.Invoke(transform.position);
        }
        
        _audioSource.clip = _sounds.startSFX;
        _audioSource.Play();

        var ps = _ps.velocityOverLifetime;
        ps.radial = _radialDonutPS;
        _ps.Play();
        if (_feedBackPlane != null)
        {
            _feedBackPlane.SetActive(false);
        }
        while (_timerController.CurrentPhase == Phase.Transparency && _timerController.CurrentFillAmount > 0f)
        {
            float alpha = _timerController.CurrentFillAmount;

            _renderer.material.SetFloat("_Alpha", alpha);
            _feedbackRenderer.material.SetFloat("_Alpha", 1f - alpha);
            
            if (_player != null)
                _player.SetDesintegratePlayer(alpha);
            if (platformWalls != null)
            {
                for (int i = 0; i < platformWalls.Length; i++)
                {
                    platformWalls[i].SetDesintegrateWall(alpha);
                }
            }
            yield return null;
        }

        _coll.enabled = false;
        if (_triggerColl != null) _triggerColl.enabled = false;

        _renderer.material.SetFloat("_Alpha", 0f);
        _feedbackRenderer.material.SetFloat("_Alpha", 1f);

        _ps.Stop();
    }

    private IEnumerator ResetTransparency()
    {
        var ps = _ps.velocityOverLifetime;
        ps.radial = 1f;
        _ps.Play();

        _coll.enabled = true;
        if (_triggerColl != null) _triggerColl.enabled = true;

        _audioSource.clip = _sounds.endSFX;
        _audioSource.Play();

        while (_timerController.CurrentPhase == Phase.ReverseTransparency && _timerController.CurrentFillAmount > 0f)
        {
            float alpha = _timerController.CurrentFillAmount;

            _renderer.material.SetFloat("_Alpha", 1f - alpha);
            _feedbackRenderer.material.SetFloat("_Alpha", alpha);
            if (_player != null)
            {
                _player.SetDesintegratePlayer(1f - alpha);
                
            }
           
            if (platformWalls != null)
            {
                for (int i = 0; i < platformWalls.Length; i++)
                {
                    platformWalls[i].SetDesintegrateWall(1f - alpha);
                }
            }
            
            yield return null;
        }

        _renderer.material.SetFloat("_Alpha", 1f);
        _feedbackRenderer.material.SetFloat("_Alpha", 0f);
        _ps.Stop();

        if (_player != null)
        {
            _player.SetCanMove(true);
            _player.StopDesintegratePlayer();
            _player = null;
            if (_feedBackPlane != null)
            {
                _feedBackPlane.SetActive(true);
            }
        }
        if (platformWalls != null)
        {
            for (int i = 0; i < platformWalls.Length; i++)
            {
                platformWalls[i].ResetDesintegrateWall();
                
            }
        }
        _isIntargeteable = false;
    }

    private IEnumerator MoveTrail()
    {
        _ps.Stop();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (_timerController.CurrentPhase == Phase.Movement && _timerController.CurrentFillAmount < 1f)
        {
            float t = _timerController.CurrentFillAmount;
            
            transform.position = Vector3.Lerp(startPos, _targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, _targetRot, t);

            if (_isPlatform)
                OnPosChanged?.Invoke(transform.position);

            yield return null;
        }
        
        transform.position = _targetPos;
        transform.rotation = _targetRot;
        OnPosChanged?.Invoke(transform.position);
    }

    private bool CanSetPlayerPlatform(PlayerNodeHandler player)
    {
        return _isCorrupted && !_isStopped && _isPlatform && player.CurrentType == _requiredNode;
    }

    private bool CanUnsetPlayerPlatform(PlayerController player)
    {
        return _isCorrupted && _isPlatform && _player != null && player == _player;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerNodeHandler playerNodeHandler) &&
            coll.TryGetComponent(out PlayerController player) &&
            CanSetPlayerPlatform(playerNodeHandler) &&
            _player == null
        )
        {
            _player = player;
            _player.SetCanMove(_timerController.CurrentPhase != Phase.Movement);
            _feedBackPlane.SetActive(true);

        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerNodeHandler playerNodeHandler) &&
            coll.TryGetComponent(out PlayerController player) &&
            CanSetPlayerPlatform(playerNodeHandler) &&
            _player == null
        )
        {
            _player = player;
            _player.SetCanMove(_timerController.CurrentPhase != Phase.Movement);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player) && CanUnsetPlayerPlatform(player))
        {
            _player = null;
            _feedBackPlane.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        OnPosChanged = null;
    }
}
