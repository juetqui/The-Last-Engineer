public class DefaultGlitcheable : Glitcheable, ICorruptionCanceler
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        _timerController.OnTimerCycleComplete += UpdateTarget;
        _timerController.OnPhaseChanged += CheckTimerPhase;
        
        if (_isCorrupted)
        {
            if(decalProjector!=null)
                decalProjector.material.SetFloat("_CorrruptedControl", 1f);
        }
        else
        {
            if (decalProjector != null)
                decalProjector.material.SetFloat("_CorrruptedControl", 0f);

            _timerController.StopCycle();
            var ps = _ps.main;
            var psVel = _ps.velocityOverLifetime;
            psVel.radial = 1f;
            ps.loop = true;
            _ps.Play();
        }
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
