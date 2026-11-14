public class InactiveState : IPlatformState
{
    private PlatformController _pc;

    public InactiveState(PlatformController pc) { _pc = pc; }

    public void Enter()
    {
        _pc.StopPassenger();
        _pc.NotifyMoving(false);
        _pc.NotifyInactive();
    }

    public void Tick(float d) { }
    public void Exit() { }

}