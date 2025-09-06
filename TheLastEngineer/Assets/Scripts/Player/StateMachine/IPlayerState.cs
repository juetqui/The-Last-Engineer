public interface IPlayerState
{
    void Enter(PlayerController player);
    void HandleInteraction(IInteractable interactable);
    void Tick();               // para l�gica frame a frame si la hubiera
    void Cancel();             // para cortar holds, etc.
    void Exit();
}
