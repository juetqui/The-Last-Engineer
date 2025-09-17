public class PlatformStateMachine
{
    private IPlatformState _current, _last;

    private PlatformController _controller;

    public IPlatformState Inactive { get; private set; }
    public IPlatformState Waiting { get; private set; }
    public IPlatformState Moving { get; private set; }
    public IPlatformState Returning { get; private set; }
    public IPlatformState Tostop { get; private set; }

    public IPlatformState Current => _current;
    public IPlatformState Last => _last;

    public PlatformStateMachine(PlatformController controller, bool startsActive)
    {
        _controller = controller;
        TransitionTo(startsActive ? Waiting : Inactive);

        Inactive = new InactiveState(_controller);
        Waiting = new WaitingState(_controller, this);
        Moving = new MovingState(_controller);
        Returning = new ReturningState(_controller, this);
        Tostop = new ToStopState(_controller, this);
    }

    public void Tick(float deltaTime) => _current?.Tick(deltaTime);

    public void TransitionTo(IPlatformState next)
    {
        if (_current == next) return;

        _last = _current;
        _current?.Exit();
        _current = next;
        _current?.Enter();
    }

    public void ToInactive() => TransitionTo(Inactive);
    public void ToWaiting() => TransitionTo(Waiting);
    public void ToMoving() => TransitionTo(Moving);
    public void ToReturning() => TransitionTo(Returning);
    public void ToStop() => TransitionTo(Tostop);
}
