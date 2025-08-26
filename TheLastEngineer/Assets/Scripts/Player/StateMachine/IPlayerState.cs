public interface IPlayerState
{
    void Enter(PlayerTDController player);
    void HandleInteraction(IInteractable interactable);
    void Tick();               // para lógica frame a frame si la hubiera
    void Cancel();             // para cortar holds, etc.
    void Exit();
}
