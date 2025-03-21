public interface IPlayerState
{
    void Enter(PlayerTDController playerController);
    void HandleInteraction();
    void Exit();

}
