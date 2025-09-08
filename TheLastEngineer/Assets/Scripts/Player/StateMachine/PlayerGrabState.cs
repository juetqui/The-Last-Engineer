using UnityEngine;

public class PlayerGrabState : IPlayerState
{
    private PlayerStateMachine _stateMachine;
    private PlayerController _player;
    private PlayerNodeHandler _playerNodeHandler;

    public PlayerGrabState(PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public void Enter(PlayerController player, PlayerNodeHandler playerNodeHandler)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        _player = player;
        _playerNodeHandler = playerNodeHandler;
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (_player.CheckForWalls()) return;

        if (interactable != null && interactable.CanInteract(_playerNodeHandler))
        {
            bool success;
            interactable.Interact(_playerNodeHandler, out success);
            
            if (success)
            {
                _player.ReleaseNode();
                if (!(interactable is Connection))
                    _player.RemoveInteractable(interactable);
                _stateMachine.TransitionToEmptyState();
                InputManager.Instance?.RumblePulse(0.25f, 1f, 0.25f);
            }
        }
        else
        {
            Debug.Log("DROP");
            _player.DropNode();
            _stateMachine.TransitionToEmptyState();
        }
    }

    public void Tick() { }

    public void Cancel() { }

    public void Exit()
    {
        _player = null;
    }
}
