public class InactiveState : IPlatformState
{
    private PlatformController _pc;

    public InactiveState(PlatformController pc) { _pc = pc; }

    public void Enter()
    {
        _pc.StopPassenger();
    }

    public void Tick(float d) { }
    public void Exit() { }

}