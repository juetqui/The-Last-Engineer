using System;
using System.Collections;
using UnityEngine;

public enum Phase { Transparency, Movement, ReverseTransparency }

public class TimerController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    [SerializeField] private float _transparencyDuration = 1f;
    [SerializeField] private float _moveDuration = 0.5f;

    private float _currentTransparencyDuration, _currentMoveDuration;
    private float _currentFillAmount = 1f;
    private bool _cycleEnabled = true;
    private Phase _currentPhase = Phase.Transparency;
    private Coroutine _dissolveCoroutine = null;

    public bool IsDebug = false;

    public Action OnTimerCycleStarted = delegate { };
    public Action OnTimerCycleComplete = delegate { };

    public float CurrentFillAmount => _currentFillAmount;
    public float CurrentDuration => _currentMoveDuration;
    public Phase CurrentPhase => _currentPhase;

    public Action<Phase> OnPhaseChanged = delegate { };

    private void Awake()
    {
        _currentTransparencyDuration = _transparencyDuration;
        _currentMoveDuration = _moveDuration;
    }

    private void Start()
    {
        PlayerController.Instance.OnNodeGrabed += SetDuration;
        _dissolveCoroutine = StartCoroutine(DissolveTimer());
    }

    //private void Update()
    //{
    //    if (IsDebug)
    //    {
    //        Debug.Log("PHASE: " + _currentPhase);
    //        Debug.Log("COROUTINE: " + _dissolveCoroutine);
    //    }
    //}

    public void StopCycle()
    {
        _cycleEnabled = false;

        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
            _dissolveCoroutine = null;
        }
        
        _currentPhase = Phase.Transparency;
        _currentFillAmount = 1f;
    }

    public void ResumeCycle()
    {
        if (_cycleEnabled) return;

        _cycleEnabled = true;
        _dissolveCoroutine = StartCoroutine(DissolveTimer());
    }

    public void SetDuration(bool hasNode, NodeType nodeType)
    {
        _currentTransparencyDuration = (!hasNode || nodeType != _requiredNode) ? _transparencyDuration : _transparencyDuration * 2f;
        _currentMoveDuration = (!hasNode || nodeType != _requiredNode) ? _moveDuration : _moveDuration * 2f;
    }

    private IEnumerator DissolveTimer()
    {
        OnTimerCycleStarted?.Invoke();
        yield return new WaitForSeconds(1.5f);

        _currentFillAmount = 1f;
        _currentPhase = Phase.Transparency;
        OnPhaseChanged?.Invoke(_currentPhase);

        while (_currentFillAmount > 0f)
        {
            _currentFillAmount -= Time.deltaTime / _currentTransparencyDuration;
            yield return null;
        }

        _currentFillAmount = 0f;
        _currentPhase = Phase.Movement;
        OnPhaseChanged?.Invoke(_currentPhase);

        while (_currentFillAmount < 1f)
        {
            _currentFillAmount += Time.deltaTime / _currentMoveDuration;
            yield return null;
        }
        _currentFillAmount = 1f;
        _currentPhase = Phase.ReverseTransparency;
        OnPhaseChanged?.Invoke(_currentPhase);

        while (_currentFillAmount > 0f)
        {
            _currentFillAmount -= Time.deltaTime / _currentTransparencyDuration;
            yield return null;
        }
        
        _currentFillAmount = 0f;
        OnTimerCycleComplete?.Invoke();

        if (_cycleEnabled)
            _dissolveCoroutine = StartCoroutine(DissolveTimer());
        else
            _dissolveCoroutine = null;
    }
}
