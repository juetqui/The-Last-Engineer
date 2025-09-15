public class ReturningState : IPlatformState
{
    private PlatformController _pc;
    private PlatformStateMachine _fsm;

    public ReturningState(PlatformController pc, PlatformStateMachine fsm)
    {
        _pc = pc;
        _fsm = fsm;
    }

    public void Enter()
    {
        _pc.Route.ForceReverse();
    }

    public void Tick(float d)
    {
        if (_pc.ReachedTarget())
        {
            if (_pc.Route.AtStart())
            {
                _pc.StopPassenger();
                _fsm.ToInactive();
                return;
            }

            _pc.Route.Advance();
        }

        _pc.MoveStep();
    }

    public void Exit() { }
}
