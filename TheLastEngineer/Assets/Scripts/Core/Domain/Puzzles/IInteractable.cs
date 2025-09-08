using UnityEngine;

public interface IInteractable
{
    public Transform Transform {  get; }
    public InteractablePriority Priority { get; }
    public bool CanInteract(PlayerNodeHandler playerNodeHandler);
    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction);
    public bool RequiresHoldInteraction { get; }
}
