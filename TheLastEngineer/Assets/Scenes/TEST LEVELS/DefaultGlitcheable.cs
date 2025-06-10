public class DefaultGlitcheable : Glitcheable, ICorruptionCanceler
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        GlitchActive.Instance.OnStopObject += StopObject;
        TimerController.Instance.OnTimerCycleStart += OnCycleStart;
        TimerController.Instance.OnTimerCycleComplete += UpdateTarget;
    }

    void Update()
    {
        UpdateTimer();
    }

    private void OnCycleStart()
    {
        if (_isStopped) return;

        _timer.fillAmount = 1f;
    }

    public void CorruptionCancel()
    {
        _isStopped = true;
        //TimerController.Instance.OnTimerCycleStart -= OnCycleStart;
        //TimerController.Instance.OnTimerCycleComplete -= UpdateTarget;
    }
}
