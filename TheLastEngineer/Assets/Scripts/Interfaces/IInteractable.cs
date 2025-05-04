using UnityEngine;

public interface IInteractable
{
    public InteractablePriority Priority { get; }
    public Transform Transform {  get; }
    public bool RequiresHoldInteraction { get; }
    public bool CanInteract(PlayerTDController player);
    public void Interact(PlayerTDController player, out bool succededInteraction);
}
