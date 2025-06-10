public class PlatesGlitcheable : Glitcheable
{
    public PlateType colorType = default;

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
        if (_isStopped) return;

        _timer.fillAmount = 1f;
    }
}

public enum PlateType
{
    None,
    Green,
    Blue,
    Purple
}
