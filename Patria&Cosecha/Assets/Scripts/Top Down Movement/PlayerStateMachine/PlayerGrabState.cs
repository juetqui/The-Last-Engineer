public class PlayerGrabState : IPlayerState
{
    private PlayerTDController _playerController = default;

    public void Enter(PlayerTDController playerController)
    {
        _playerController = playerController;
        _playerController.CheckCurrentNode();
    }

    public void HandleInteraction()
    {
        if (_playerController.IsInConnectionArea)
        {
            _playerController.PlaceNode();
        }
        else if (_playerController.IsInCombinationArea)
        {
            _playerController.PlaceInMachine();
        }
        else
        {
            _playerController.DropNode();
        }

        _playerController.View.GrabNode();
        _playerController.SetState(_playerController.EmptyState);
    }

    public void Exit()
    {
        _playerController = null;
    }
}
