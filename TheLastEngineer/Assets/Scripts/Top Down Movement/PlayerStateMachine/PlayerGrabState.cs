using UnityEngine;

public class PlayerGrabState : IPlayerState
{
    private PlayerTDController _playerController = default;

    public void Enter(PlayerTDController playerController)
    {
        _playerController = playerController;
        _playerController.DropOrGrabNode(true);
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (!_playerController.DropAvailable || _playerController.CheckForWalls()) return;

        if (interactable != null && interactable.CanInteract(_playerController))
        {
            bool succededInteraction = default;
            interactable.Interact(_playerController, out succededInteraction);

            if (succededInteraction)
            {
                _playerController.DropOrGrabNode(false);
                _playerController.ReleaseNode();

                if (!(interactable is GenericConnectionController) && !(interactable is SpecificConnectionController))
                    _playerController.RemoveInteractable(interactable);

                _playerController.SetState(_playerController.EmptyState);
                InputManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
            }
        }
        else
        {
            _playerController.DropOrGrabNode(false);
            _playerController.DropNode();
            _playerController.SetState(_playerController.EmptyState);
        }
    }

    public void Exit()
    {
        _playerController = null;
    }
}
