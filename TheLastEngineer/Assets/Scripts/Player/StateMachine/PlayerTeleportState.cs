public class PlayerTeleportState : IPlayerState
{
    private PlayerStateMachine _fsm = default;
    private PlayerController _player = default;

    public PlayerTeleportState(PlayerStateMachine fsm)
    {
        _fsm = fsm;
    }

    public void Enter(PlayerController player, PlayerNodeHandler nodeHanlder)
    {
        _player = player;
        _player.OnTeleported += _fsm.TransitionToDissolving;
        _player.StartTeleport();
    }

    public void Tick()
    {
        _player.Teleport();
    }

    public void Exit()
    {
        _player.OnTeleported -= _fsm.TransitionToDissolving;
        _player = null;
    }

    public void HandleInteraction(IInteractable interactable) { }

    public void Cancel() { }
}
