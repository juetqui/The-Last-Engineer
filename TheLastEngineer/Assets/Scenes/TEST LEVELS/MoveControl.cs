using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveControl : MonoBehaviour
{
    public static MoveControl Instance = null;

    [SerializeField] private NodeType _requiredType = NodeType.Green;
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private float _nodeDuration = 2f;

    private Image _timer = default;
    private bool _canMove = true, _isStopped = false;
    private float _currentDuration = default;

    public Action OnChangePositions = delegate { };

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _timer = GetComponentInChildren<Image>();
        _timer.fillAmount = 1f;

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
        _currentDuration = (!hasNode || nodeType != _requiredType) ? _defaultDuration : _nodeDuration;
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

        OnChangePositions?.Invoke();

        _timer.fillAmount = 1f;
        _canMove = true;
    }
}
