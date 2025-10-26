using UnityEngine;

public class PlayerGrabState : IPlayerState
{
    private PlayerStateMachine _stateMachine;
    private PlayerController _player;
    private PlayerNodeHandler _playerNodeHandler;

    private IInteractable _target = null;

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
        _target = null;
    }

    public void Tick()
    {
        _player.GetClosestGlitcheable();
    }

    public void Cancel() { }

    public void Exit()
    {
        _player = null;
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (interactable is Inspectionable inspectionable)
        {
            Debug.Log("Interacting with Inspectionable");
            _target = interactable;
            inspectionable.OnFinished += RemoveInteractable;

            return;
        }

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
            _player.SetTeleport(teleport.TargetPos);
            _player.RemoveInteractable(interactable);
            _stateMachine.TransitionToDissolving();
        }
        else if (interactable is Glitcheable glitcheable)
        {
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

    private void RemoveInteractable()
    {
        if (_target is Inspectionable inspectionable)
            inspectionable.OnFinished -= RemoveInteractable;

        _player.RemoveInteractable(_target);
        Debug.Log("Inspection Finished");
    }
}
