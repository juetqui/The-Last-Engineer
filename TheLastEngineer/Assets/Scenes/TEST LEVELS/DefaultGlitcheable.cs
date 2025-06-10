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
    public void CorruptionCheck() { }

    public void CorruptionRestore()
    {
        _isStopped = false;
        print("hola");
    }
    public void CorruptionCancel()
    {
        print("cancelado");
        _isStopped = true;
        //TimerController.Instance.OnTimerCycleStart -= OnCycleStart;
        //TimerController.Instance.OnTimerCycleComplete -= UpdateTarget;
    }
}
