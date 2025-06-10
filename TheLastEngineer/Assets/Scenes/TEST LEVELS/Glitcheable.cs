using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Glitcheable : MonoBehaviour
{
    [SerializeField] private Transform _feedbackPos;
    [SerializeField] protected List<Transform> _newPosList;
    [SerializeField] protected TimerController _timerController;

    protected List<Transform> _currentList = default;
    protected Image _timer = default;
    protected bool _canMove = true;
    public bool _isStopped = false;
    protected int _index = 0;

    private Vector3 _targetPos = default, _feedBackStartPos = default, _feedBackCurrentPos = default;
    private Color _originalColor = default;
    private Coroutine _moveTrail = null;

    public bool IsStopped { get { return _isStopped; } }
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

        if (_isStopped) _timer.color = Color.magenta;
        else
        {
            _timer.color = _originalColor;
            
            if (_feedbackPos != null)
            {
                float t = 1f - _timerController.CurrentFillAmount;
                _feedbackPos.position = Vector3.Lerp(_feedBackCurrentPos, _targetPos, t);
            }
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


    protected void UpdateTarget()
    {
        if (_isStopped) return;

        if (_index == _currentList.Count - 1)
            _index = 0;
        else
            _index++;

        transform.position = _targetPos;
        transform.rotation = _currentList[_index].rotation;

        _targetPos = _currentList[_index].position;

        if (_feedbackPos != null)
            _feedBackCurrentPos = _feedbackPos.position;
    }

    public void PositionReset()
    {
        transform.position = _currentList[_currentList.Count - 1].position;
        transform.rotation = _currentList[_currentList.Count-1].rotation;
        _isStopped = false;
        _timer.fillAmount = 1;
        _index = 0;

        if (_feedbackPos != null)
            _feedBackCurrentPos = _feedBackStartPos;
    }
}
