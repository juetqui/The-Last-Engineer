using UnityEngine;

public abstract class Connection<T> : MonoBehaviour, IInteractable, IConnectable
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.High;
    public Transform Transform => transform;
    #endregion

    protected MainTM _mainTM = default;

    public abstract void SetSecTM(T secTM);
    protected abstract void SetNode(NodeController node);
    public abstract void UnsetNode(NodeController node = null);

    public void SetMainTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }

    public abstract bool CanInteract(PlayerTDController player);

    public void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (CanInteract(player) && player.GetCurrentNode() != null)
        {
            NodeController node = player.GetCurrentNode();
            SetNode(node);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }
    }
}
