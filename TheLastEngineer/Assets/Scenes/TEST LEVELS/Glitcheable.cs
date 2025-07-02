using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public abstract class Glitcheable : MonoBehaviour
{
    [SerializeField] private Collider _coll;
    [SerializeField] private Collider _triggerColl;
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private Transform _feedbackPos;
    [SerializeField] protected List<Transform> _newPosList;
    [SerializeField] protected TimerController _timerController;
    [SerializeField] protected bool _isPlatform = false;
    [SerializeField] protected bool _isCorrupted = true;

    protected List<Transform> _currentList = default;
    protected bool _canMove = true;
    protected bool _isStopped = false;
    protected int _index = 0;

    private PlayerTDController _player = null;
    private Renderer _renderer = default, _feedbackRenderer = default;
    private Coroutine _coroutine = null;
    private NodeType _requiredNode = NodeType.Corrupted;
    private Vector3 _targetPos = default, _feedBackStartPos = default, _feedBackCurrentPos = default;
    private Quaternion _targetRot = default;
    private Color _originalColor = default;

    public bool IsStopped { get { return _isStopped; } }

    public Action<Vector3> OnPosChanged = delegate { };

    public int GetOnPosChangedHandlerCount()
    {
        return OnPosChanged?.GetInvocationList().Length ?? 0;
    }

    protected void OnAwake()
    {
        _renderer = GetComponent<Renderer>();
        _feedbackRenderer = _feedbackPos.GetComponent<Renderer>();

        _currentList = _newPosList;
        _targetPos = _currentList[_index].position;
        _targetRot = _currentList[_index].rotation;

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

    protected void StopObject(Glitcheable glitcheable)
    {
        if (glitcheable != this)
        {
            _isStopped = false;
            return;
        }

        _isStopped = !_isStopped;
    }

    public bool ChangeCorruptionState(NodeType nodeType, bool newState)
    {
        if (newState == _isCorrupted) return false;

        _isCorrupted = newState;

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
        if (_player != null)
        {
            _player.SetCanMove(false);
            OnPosChanged?.Invoke(transform.position);
        }

        _coll.enabled = false;
        _triggerColl.enabled = false;

        var ps = _ps.velocityOverLifetime;
        ps.radial = -4.91f;
        _ps.Play();

        while (_timerController.CurrentPhase == Phase.Transparency && _timerController.CurrentFillAmount > 0f)
        {
            float alpha = _timerController.CurrentFillAmount;

            _renderer.material.SetFloat("_Alpha", alpha);
            _feedbackRenderer.material.SetFloat("_Alpha", 1f - alpha);

            yield return null;
        }

        _ps.Stop();
    }

    private IEnumerator ResetTransparency()
    {
        var ps = _ps.velocityOverLifetime;
        ps.radial = 1f;

        _ps.Play();

        while (_timerController.CurrentPhase == Phase.ReverseTransparency && _timerController.CurrentFillAmount > 0f)
        {
            float alpha = _timerController.CurrentFillAmount;
            
            _renderer.material.SetFloat("_Alpha", 1f - alpha);
            _feedbackRenderer.material.SetFloat("_Alpha", alpha);

            yield return null;
        }

        _coll.enabled = true;
        _triggerColl.enabled = true;
        _ps.Stop();

        if (_player != null)
        {
            _player.SetCanMove(true);
            _player.UnsetPlatform(this);
            _player = null;
        }
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

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            if (_isPlatform && player.GetCurrentNodeType() == _requiredNode)
            {
                _player = player;
                _player.SetPlatform(this);
                _player.SetCanMove(_timerController.CurrentPhase != Phase.Movement);
            }
            else
                player.CorruptionCollided();
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            if (_isPlatform && _player != null && player == _player)
            {
                _player.UnsetPlatform(this);
                _player = null;
            }
        }
    }

    private void OnDestroy()
    {
        OnPosChanged = null;
    }
}
