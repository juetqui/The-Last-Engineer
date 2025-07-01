using UnityEngine;

public class DefaultGlitcheable : Glitcheable, ICorruptionCanceler
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        GlitchActive.Instance.OnStopObject += StopObject;
        _timerController.OnTimerCycleComplete += UpdateTarget;
        _timerController.OnPhaseChanged += CheckTimerPhase;
    }

    private void OnCycleStart()
    {
        if (_isStopped || !_isCorrupted) return;
    }
    
    public void CorruptionCheck() { }

    public void CorruptionRestore()
    {
        _isStopped = false;
    }

    public void CorruptionCancel()
    {
        _isStopped = true;
    }
}
