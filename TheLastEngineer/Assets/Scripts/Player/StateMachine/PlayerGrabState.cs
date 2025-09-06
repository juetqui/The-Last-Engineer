public class PlayerGrabState : IPlayerState
{
    private PlayerController _player;

    public void Enter(PlayerController player)
    {
        _player = player;
        _player.DropOrGrabNode(true);
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (!_player.DropAvailable || _player.CheckForWalls()) return;

        if (interactable != null && interactable.CanInteract(_player))
        {
            bool success;
            interactable.Interact(_player, out success);
            if (success)
            {
                _player.DropOrGrabNode(false);
                _player.ReleaseNode();

                if (!(interactable is Connection))
                    _player.RemoveInteractable(interactable);

                _player.SetState(_player.EmptyState);
                InputManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
            }
        }
        else
        {
            _player.DropOrGrabNode(false);
            _player.DropNode();
            _player.SetState(_player.EmptyState);
        }
    }

    public void Tick() { }
    public void Cancel() { }
    public void Exit() { _player = null; }
}
