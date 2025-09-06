using UnityEngine;

public interface IInteractable
{
    public Transform Transform {  get; }
    public InteractablePriority Priority { get; }
    public bool CanInteract(PlayerController player);
    public void Interact(PlayerController player, out bool succededInteraction);
    public bool RequiresHoldInteraction { get; }
}
