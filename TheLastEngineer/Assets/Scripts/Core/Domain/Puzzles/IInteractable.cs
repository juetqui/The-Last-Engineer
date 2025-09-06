using UnityEngine;

public interface IInteractable
{
    public Transform Transform {  get; }
    public InteractablePriority Priority { get; }
    public bool CanInteract(PlayerTDController player);
    public void Interact(PlayerTDController player, out bool succededInteraction);
    public bool RequiresHoldInteraction { get; }
}
