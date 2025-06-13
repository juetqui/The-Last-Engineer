using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Glitcheable : MonoBehaviour
{
    [SerializeField] private Transform _feedbackPos;
    [SerializeField] protected List<Transform> _newPosList;
    [SerializeField] protected TimerController _timerController;
    [SerializeField] protected bool _isPlatform = false;

    protected List<Transform> _currentList = default;
    protected Image _timer = default;
    protected bool _canMove = true;
    protected bool _isStopped = false;
    protected int _index = 0;

    private Vector3 _targetPos = default, _feedBackStartPos = default, _feedBackCurrentPos = default;
    private Color _originalColor = default;

    public Action<Vector3> OnPosChanged = delegate { };

    public bool IsStopped { get { return _isStopped; } }

    public int GetOnPosChangedHandlerCount()
    {
        return OnPosChanged?.GetInvocationList().Length ?? 0;
    }

    protected void OnAwake()
    {
        _timer = GetComponentInChildren<Image>();
        _currentList = _newPosList;
        _targetPos = _currentList[_index].position;
        _originalColor = _timer.color;

        if (_feedbackPos != null)
            _feedBackCurrentPos = _feedBackStartPos = _feedbackPos.position;
    }

    protected void UpdateTimer()
    {
        _timer.fillAmount = _timerController.CurrentFillAmount;

        if (_isStopped) _timer.color = Color.yellow;
        else _timer.color = _originalColor;

        StartCoroutine(MoveTrail());
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

    protected void UpdateTarget()
    {
        if (_isStopped) return;

        if (_index == _currentList.Count - 1) _index = 0;
        else _index++;

        transform.position = _targetPos;
        transform.rotation = _currentList[_index].rotation;

        if (_isPlatform) OnPosChanged?.Invoke(_targetPos);

        _targetPos = _currentList[_index].position;

        if (_feedbackPos != null)
            _feedBackCurrentPos = _feedbackPos.position;
    }

    public void PositionReset()
    {
        transform.position = _currentList[_currentList.Count - 1].position;
        transform.rotation = _currentList[_currentList.Count - 1].rotation;
        _isStopped = false;
        _timer.fillAmount = 1;
        _index = 0;

        if (_feedbackPos != null)
            _feedBackCurrentPos = _feedBackStartPos;
    }

    private IEnumerator MoveTrail()
    {
        float t = 1f - _timerController.CurrentFillAmount;

        if (_feedbackPos != null)
        {
            if (_isStopped)
                _feedbackPos.position = Vector3.Lerp(_feedbackPos.position, _feedBackCurrentPos, t);
            else
                _feedbackPos.position = Vector3.Lerp(_feedBackCurrentPos, _targetPos, t);
        }

        yield return null;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            if (_isPlatform && (player.GetCurrentNode() == null || player.GetCurrentNode().NodeType == NodeType.Purple))
                player.SetPlatform(this);
            else
                player.CorruptionCollided();
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            if (_isPlatform && (player.GetCurrentNode() == null || player.GetCurrentNode().NodeType == NodeType.Purple))
                player.UnSetPlatform(this);
        }
    }

    private void OnDestroy()
    {
        OnPosChanged = null;
    }
}
