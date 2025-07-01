using System;
using System.Collections;
using UnityEngine;

public enum Phase { Transparency, Movement, ReverseTransparency }

public class TimerController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    [SerializeField] private float _transparencyDuration = 1f;
    [SerializeField] private float _moveDuration = 0.5f;
    [SerializeField] private float _nodeDuration = 2f;

    private float _currentTransparencyDuration, _currentMoveDuration;
    private float _currentFillAmount = 1f;
    private Phase _currentPhase = Phase.Transparency;

    public event Action OnTimerCycleComplete;

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
        PlayerTDController.Instance.OnNodeGrabed += SetDuration;
        StartCoroutine(DissolveTimer());
    }

    public void SetDuration(bool hasNode, NodeType nodeType)
    {
        _currentTransparencyDuration = (!hasNode || nodeType != _requiredNode) ? _transparencyDuration : _transparencyDuration * 2f;
        _currentMoveDuration = (!hasNode || nodeType != _requiredNode) ? _moveDuration : _moveDuration * 2f;
    }

    private IEnumerator DissolveTimer()
    {
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

        yield return new WaitForSeconds(3f);

        StartCoroutine(DissolveTimer());
    }
}
