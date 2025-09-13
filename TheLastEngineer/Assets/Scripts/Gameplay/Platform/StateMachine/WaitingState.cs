using UnityEngine;

public class WaitingState : IPlatformState
{
    private PlatformController _pc;
    private PlatformStateMachine _FSM;

    private float _timer = 0f;

    public WaitingState(PlatformController pc, PlatformStateMachine fSM)
    {
        _pc = pc;
        _FSM = fSM;
    }

    public void Enter()
    {
        _pc.BeginWait();
        _pc.StopPassenger();
        _timer = 0f;
    }

    public void Tick(float d)
    {
        _timer += d;

        if (_timer >= _pc.WaitTimer)
            _FSM.TransitionTo(_FSM.Moving);
    }

    public void Exit() { }
}