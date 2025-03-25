using UnityEngine;

public class PlayerGrabState : IPlayerState
{
    private PlayerTDController _playerController = default;

    public void Enter(PlayerTDController playerController)
    {
        _playerController = playerController;
        _playerController.CheckCurrentNode();
    }

    public void HandleInteraction(IInteractable interactable)
    {
        Debug.Log(interactable);
        if (interactable != null && interactable.CanInteract(_playerController))
        {
            bool succededInteraction = default;
            interactable.Interact(_playerController, out succededInteraction);

            if (succededInteraction)
            {
                _playerController.ReleaseNode(interactable);
                _playerController.SetState(_playerController.EmptyState);
            }
        }
        else
        {
            Debug.Log("DROP");
            _playerController.DropNode();
            _playerController.SetState(_playerController.EmptyState);
        }
    }

    public void Exit()
    {
        _playerController = null;
    }
}
