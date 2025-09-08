public interface IPlayerState
{
    void Enter(PlayerController player, PlayerNodeHandler playerNodeHandler);
    void HandleInteraction(IInteractable interactable);
    void Tick();
    void Cancel();
    void Exit();
}
