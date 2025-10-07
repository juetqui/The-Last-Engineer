public class PlayerStateMachine
{
    private IPlayerState _currentState, _lastState;
    
    private PlayerController _controller;
    private PlayerNodeHandler _nodeHandler;

    private PlayerEmptyState _emptyState;
    private PlayerGrabState _grabState;
    private PlayerDissolvingState _dissolvingState;
    private PlayerTeleportState _teleportState;
    
    public IPlayerState CurrentState => _currentState;
    public IPlayerState LastState => _lastState;

    public PlayerStateMachine(PlayerController controller, PlayerNodeHandler nodeHandler)
    {
        if (controller == null)
            throw new System.ArgumentNullException(nameof(controller));

        _controller = controller;
        _nodeHandler = nodeHandler;

        _emptyState = new PlayerEmptyState(this);
        _grabState = new PlayerGrabState(this);
        _dissolvingState = new PlayerDissolvingState(this);
        _teleportState = new PlayerTeleportState(this);

        TransitionToState(_emptyState);
    }

    public void Tick()
    {
        _currentState?.Tick();
    }

    public void TransitionToState(IPlayerState newState)
    {
        if (_currentState != _dissolvingState && _currentState != _teleportState)
            _lastState = _currentState;

        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(_controller, _nodeHandler);
    }

    public void TransitionToEmptyState()
    {
        TransitionToState(_emptyState);
    }

    public void TransitionToGrabState(NodeController node)
    {
        _controller.PickUpNode(node);
        TransitionToState(_grabState);
    }

    public void TransitionToDissolving()
    {
        TransitionToState(_dissolvingState);
    }
    public void TransitionToTeleport()
    {
        TransitionToState(_teleportState);
    }
}
