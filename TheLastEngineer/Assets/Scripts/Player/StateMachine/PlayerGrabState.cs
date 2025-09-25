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

    public void Tick() { }

    public void Cancel() { }

    public void Exit()
    {
        _player = null;
    }

    public void HandleInteraction(IInteractable interactable)
    {
        Debug.Log(interactable);

        if (_player.CheckForWalls()) return;

        if (interactable == null || !interactable.CanInteract(_playerNodeHandler))
        {
            HandleFailedInteraction();
            return;
        }

        interactable.Interact(_playerNodeHandler, out bool success);

        if (!success)
        {
            HandleFailedInteraction();
            return;
        }

        if (interactable is PlatformTeleport teleport)
        {
            _player.SetPos(teleport.TargetPos);
            _player.AddInteractable(teleport.TargetPlatform);
            _player.RemoveInteractable(interactable);
        }
        else if (interactable is Glitcheable glitcheable)
        {
            Debug.Log("GLITCHEABLE");
            _playerNodeHandler.OnGlitchChange(glitcheable);
        }
        else if (interactable is Connection)
        {
            _player.ReleaseNode();
            _stateMachine.TransitionToEmptyState();
        }
        
        InputManager.Instance?.RumblePulse(0.25f, 1f, 0.25f);
    }

    private void HandleFailedInteraction()
    {
        _player.DropNode();
        _stateMachine.TransitionToEmptyState();
    }
}
