using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Glitcheable : MonoBehaviour
{
    [SerializeField] protected List<Transform> _newPosList;
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private float _nodeDuration = 2f;

    protected List<Transform> _currentList = default;
    protected Image _timer = default;
    protected bool _canMove = true, _isStopped = false;
    protected int _index = 0;
    
    private Vector3 _targetPos = default;
    //private Vector3 _originalPos = default;
    //private Quaternion _targetRot = default;
    //private float _currentDuration = default;

    public bool IsStopped { get { return _isStopped; } }

    protected void OnAwake()
    {
        _timer = GetComponentInChildren<Image>();

        _currentList = _newPosList;
        _targetPos = _currentList[_index].position;
        //_targetRot = _currentList[_index].rotation;

        //_originalPos = transform.position;
        //_currentDuration = _defaultDuration;
    }

    protected void UpdateTimer()
    {
        if (!_isStopped)
        {
            _timer.fillAmount = TimerController.Instance.CurrentFillAmount;
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
        //_targetRot = _currentList[_index].rotation;
    }

    //protected IEnumerator StartTimer()
    //{
    //    _canMove = false;

    //    while (_timer.fillAmount > 0f)
    //    {
    //        if (!_isStopped)
    //            _timer.fillAmount -= _currentDuration * Time.deltaTime;
    //        else
    //            _timer.fillAmount -= 0f;

    //        yield return null;
    //    }

    //    _timer.fillAmount = 0f;

    //    yield return new WaitForSeconds(0.25f);

    //    UpdateTarget();

    //    _timer.fillAmount = 1f;
    //    _canMove = true;
    //}
}
