public class PlayerEmptyState : IPlayerState
{
    private PlayerTDController _playerController = default;

    public void Enter(PlayerTDController playerController)
    {
        _playerController = playerController;
        _playerController.CheckCurrentNode();
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (interactable == null) return;

        bool succededInteraction = default;
        interactable.Interact(_playerController, out succededInteraction);

        if (succededInteraction && interactable is ElectricityNode node)
        {
            _playerController.PickUpNode(node);
            _playerController.SetState(_playerController.GrabState);
        }
        else
        {
            InputManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
        }
    }

    public void Exit()
    {
        _playerController = null;
    }
}
