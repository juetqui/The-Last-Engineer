using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Glitcheable : MonoBehaviour
{
    [SerializeField] private List<Transform> _newPosList;
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private float _nodeDuration = 2f;

    private Vector3 _originalPos = default, _targetPos = default;
    private Image _timer = default;
    private bool _disposed = false, _canMove = true, _isStopped = false;
    private float _currentDuration = default;
    private int _index = 0;

    private void Awake()
    {
        _timer = GetComponentInChildren<Image>();

        _targetPos = _newPosList[_index].position;
        _index++;
        _originalPos = transform.position;
        _currentDuration = _defaultDuration;
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += CheckNode;
        GlitchActive.Instance.OnStopObject += StopObject;
    }

    void Update()
    {
        if (_canMove && !_isStopped)
        {
            StartCoroutine(StartTimer());
        }
    }

    private void StopObject(Glitcheable glitcheable)
    {
        if (glitcheable != this)
        {
            _isStopped = false;
            return;
        }

        _isStopped = !_isStopped;
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        _currentDuration = (!hasNode || nodeType != NodeType.Green) ? _defaultDuration : _nodeDuration;
    }

    private void UpdateTarget()
    {
        if (_index == _newPosList.Count - 1)
            _index = 0;
        else
            _index++;

        transform.position = _targetPos;

        //_disposed = !_disposed;
        //_targetPos = _disposed ? _originalPos : _newPosList[_index].position;
        _targetPos = _newPosList[_index].position;
    }

    private IEnumerator StartTimer()
    {
        _canMove = false;

        while (_timer.fillAmount > 0f)
        {
            if (!_isStopped)
                _timer.fillAmount -= _currentDuration * Time.deltaTime;
            else
                _timer.fillAmount -= 0f;

            yield return null;
        }

        _timer.fillAmount = 0f;

        yield return new WaitForSeconds(0.25f);

        UpdateTarget();

        _timer.fillAmount = 1f;
        _canMove = true;
    }
}
