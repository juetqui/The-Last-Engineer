using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Glitcheable : MonoBehaviour
{
    [SerializeField] protected List<Transform> _newPosList;
    [SerializeField] protected TimerController _timerController;
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private float _nodeDuration = 2f;

    protected List<Transform> _currentList = default;
    protected Image _timer = default;
    protected bool _canMove = true, _isStopped = false;
    protected int _index = 0;

    private Vector3 _targetPos = default;
    private Color _originalColor = default;

    public bool IsStopped { get { return _isStopped; } }

    protected void OnAwake()
    {
        _timer = GetComponentInChildren<Image>();

        _currentList = _newPosList;
        _targetPos = _currentList[_index].position;
        _originalColor = _timer.color;
    }

    protected void UpdateTimer()
    {
        _timer.fillAmount = _timerController.CurrentFillAmount;
        
        if (_isStopped) _timer.color = Color.magenta;
        else _timer.color = _originalColor;
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
    }

    public void PositionReset()
    {
        transform.position = _currentList[_currentList.Count - 1].position;
        transform.rotation = _currentList[_currentList.Count-1].rotation;
        _isStopped = false;
        _timer.fillAmount = 1;
        _index = 0;

    }
}
