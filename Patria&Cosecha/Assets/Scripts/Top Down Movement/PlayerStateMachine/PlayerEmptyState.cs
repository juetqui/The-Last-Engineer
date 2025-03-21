public class PlayerEmptyState : IPlayerState
{
    private PlayerTDController _playerController = default;

    public void Enter(PlayerTDController playerController)
    {
        _playerController = playerController;
        _playerController.CheckCurrentNode();
    }

    public void HandleInteraction()
    {
        if (_playerController.HasNode() && !_playerController.CheckForWalls())
        {
            _playerController.ChangeNode();
            _playerController.SetState(_playerController.GrabState);
        }
        else if (_playerController.IsInCombinerArea)
        {
            _playerController.ActivateCombiner();
        }
    }

    public void Exit()
    {
        _playerController = null;
    }
}
