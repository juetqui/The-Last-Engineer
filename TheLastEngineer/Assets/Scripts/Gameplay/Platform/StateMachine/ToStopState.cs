public class ToStopState : IPlatformState
{
    private PlatformController _pc;
    private PlatformStateMachine _fsm;

    public ToStopState(PlatformController pc, PlatformStateMachine fsm)
    {
        _pc = pc;
        _fsm = fsm;
    }

    public void Enter()
    {
        _pc.isStopped = false;
        _pc.DecelerateToTarget(_pc.CurrentTarget);
    }

    public void Tick(float d)
    {
        if (_pc.ReachedTarget())
        {
            _pc.StopImmediate();
            _pc.StopPassenger();
            _fsm.ToInactive();
            _pc.Route.Advance();
            _pc.isStopped = true;
            return;
        }

        _pc.MoveStep();
    }

    public void Exit()
    {

    }
}
