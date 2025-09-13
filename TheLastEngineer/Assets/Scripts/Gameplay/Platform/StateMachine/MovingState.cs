public class MovingState : IPlatformState
{
    private PlatformController _pc;

    public MovingState(PlatformController pc) { _pc = pc; }

    public void Enter() { }

    public void Tick(float d)
    {
        if (_pc.ReachedTarget())
        {
            _pc.StopPassenger();
            _pc.AdvanceRouteAndWait();
            return;
        }

        _pc.MoveStep();
    }

    public void Exit() { }
}