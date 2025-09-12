//public class PlatformStateMachine
//{
//    private IPlatformState _current, _last;

//    private readonly PlatformController _controller;

//    // Estados cacheados (evita allocs)
//    public readonly InactiveState Inactive = new InactiveState();
//    public readonly WaitingState Waiting = new WaitingState();
//    public readonly MovingState Moving = new MovingState();

//    public IPlatformState Current => _current;
//    public IPlatformState Last => _last;

//    public PlatformStateMachine(PlatformController controller, bool startsActive)
//    {
//        _controller = controller;
//        TransitionTo(startsActive ? Waiting : Inactive);
//    }

//    public void Tick() => _current?.Tick();

//    public void TransitionTo(IPlatformState next)
//    {
//        if (_current == next) return;
//        _last = _current;
//        _current?.Exit();
//        _current = next;
//        _current?.Enter();
//    }

//    // Azúcares para legibilidad
//    public void ToInactive() => TransitionTo(Inactive);
//    public void ToWaiting() => TransitionTo(Waiting);
//    public void ToMoving() => TransitionTo(Moving);
//}
