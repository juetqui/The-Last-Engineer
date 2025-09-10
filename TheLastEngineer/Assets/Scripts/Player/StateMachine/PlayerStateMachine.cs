using System.Diagnostics;
using UnityEngine;

public class PlayerStateMachine
{
    private IPlayerState _currentState, _lastState;
    
    private PlayerController _controller;
    private PlayerNodeHandler _nodeHandler;
    
    public IPlayerState CurrentState => _currentState;
    public IPlayerState LastState => _lastState;

    public PlayerStateMachine(PlayerController controller, PlayerNodeHandler nodeHandler)
    {
        if (controller == null)
            throw new System.ArgumentNullException(nameof(controller));

        _controller = controller;
        _nodeHandler = nodeHandler;
        TransitionToState(new PlayerEmptyState(this));
    }

    public void Tick()
    {
        _currentState?.Tick();
    }

    public void TransitionToState(IPlayerState newState)
    {
        _lastState = _currentState;
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(_controller, _nodeHandler);
    }

    public void TransitionToEmptyState()
    {
        TransitionToState(new PlayerEmptyState(this));
    }

    public void TransitionToGrabState(NodeController node)
    {
        var grabState = new PlayerGrabState(this);
        _controller.PickUpNode(node);
        TransitionToState(grabState);
    }
}
