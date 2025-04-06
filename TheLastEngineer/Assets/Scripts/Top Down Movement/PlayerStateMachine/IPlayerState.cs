public interface IPlayerState
{
    void Enter(PlayerTDController playerController);
    void HandleInteraction(IInteractable interactable);
    void Exit();

}
