using System;
using System.Collections;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [SerializeField] private float _defaultDuration = 1f;
    [SerializeField] private float _nodeDuration = 2f;

    private float _currentDuration;
    private float _currentFillAmount = 1f;

    public event Action OnTimerCycleStart;
    public event Action OnTimerCycleComplete;

    public float CurrentFillAmount => _currentFillAmount;

    private void Awake()
    {
        _currentDuration = _defaultDuration;
    }

    private void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += SetDuration;
        StartCoroutine(TimerRoutine());
    }

    public void SetDuration(bool hasNode, NodeType nodeType)
    {
        _currentDuration = (!hasNode || nodeType != NodeType.Green) ? _defaultDuration : _nodeDuration;
    }

    private IEnumerator TimerRoutine()
    {
        while (true)
        {
            OnTimerCycleStart?.Invoke();
            _currentFillAmount = 1f;

            while (_currentFillAmount > 0f)
            {
                _currentFillAmount -= Time.deltaTime / _currentDuration;
                yield return null;
            }

            _currentFillAmount = 0f;

            yield return new WaitForSeconds(0.25f);

            OnTimerCycleComplete?.Invoke();
        }
    }
}
