using UnityEngine.Rendering.Universal;

public class DefaultGlitcheable : Glitcheable, ICorruptionCanceler
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        GlitchActive.Instance.OnStopObject += StopObject;

        if (_isCorrupted)
        {
            _timerController.OnTimerCycleComplete += UpdateTarget;
            _timerController.OnPhaseChanged += CheckTimerPhase;
        }
        else
        {
            var ps = _ps.main;
            var psVel = _ps.velocityOverLifetime;
            psVel.radial = 1f;
            ps.loop = true;
            _ps.Play();
        }
        print(decalProjector.material);
        if (IsCorrupted)
        {
            decalProjector.material.SetFloat("_CorrruptedControl", 1f);
        }
        else
        {
            decalProjector.material.SetFloat("_CorrruptedControl", 0f);
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
