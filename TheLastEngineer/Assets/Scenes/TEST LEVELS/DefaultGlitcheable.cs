public class DefaultGlitcheable : Glitcheable, ICorruptionCanceler
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        GlitchActive.Instance.OnStopObject += StopObject;
        _timerController.OnTimerCycleStart += OnCycleStart;
        _timerController.OnTimerCycleComplete += UpdateTarget;
    }

    void Update()
    {
        UpdateTimer();
    }

    private void OnCycleStart()
    {
        if (_isStopped || !_isCorrupted) return;

        //_timer.fillAmount = 1f;
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
