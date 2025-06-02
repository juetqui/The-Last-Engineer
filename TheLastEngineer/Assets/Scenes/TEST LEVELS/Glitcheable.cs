using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Glitcheable : MonoBehaviour
{
    [SerializeField] private Transform _newPos;
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private float _nodeDuration = 2f;

    private Vector3 _originalPos = default, _targetPos = default;
    private Image _timer = default;
    private bool _disposed = false, _canMove = true, _isStopped = false;
    private float _currentDuration = default;

    private void Awake()
    {
        _timer = GetComponentInChildren<Image>();

        _targetPos = _newPos.position;
        _originalPos = transform.position;
        _currentDuration = _defaultDuration;
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += CheckNode;
    }

    void Update()
    {
        if (_canMove && !_isStopped)
        {
            StartCoroutine(StartTimer());
        }
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        _currentDuration = (!hasNode || nodeType != NodeType.Green) ? _defaultDuration : _nodeDuration;
    }

    private void UpdateTarget()
    {
        transform.position = _targetPos;

        _disposed = !_disposed;
        _targetPos = _disposed ? _originalPos : _newPos.position;
    }

    private IEnumerator StartTimer()
    {
        _canMove = false;

        while (_timer.fillAmount > 0f)
        {
            _timer.fillAmount -= _currentDuration * Time.deltaTime;
            yield return null;
        }

        _timer.fillAmount = 0f;

        yield return new WaitForSeconds(0.25f);

        UpdateTarget();

        _timer.fillAmount = 1f;
        _canMove = true;
    }
}
