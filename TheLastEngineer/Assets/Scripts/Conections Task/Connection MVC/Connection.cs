using UnityEngine;

public abstract class Connection<T> : MonoBehaviour, IInteractable, IConnectable
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.High;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    protected MainTM _mainTM = default;
    protected NodeController _recievedNode = null;

    public abstract void SetSecTM(T secTM);
    protected abstract void SetNode(NodeController node);
    public abstract void UnsetNode(NodeController node = null);

    public void SetMainTM()
    {
        _mainTM = MainTM.Instance;
    }

    public abstract bool CanInteract(PlayerTDController player);

    public void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (_recievedNode != null)
        {
            succededInteraction = false;
        }
        else if (CanInteract(player) && player.GetCurrentNode() != null)
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
