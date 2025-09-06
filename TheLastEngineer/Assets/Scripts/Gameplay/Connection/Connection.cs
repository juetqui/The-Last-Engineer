using System;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour, IInteractable, IConnectable
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.High;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    [SerializeField] private NodeController _recievedNode;
    [SerializeField] private Transform _nodePos;
    [SerializeField] private List<DoorsView> _doorsView;
    public bool StartsConnected { get; private set; }
    public Action<NodeType, bool> OnNodeConnected;

    private void Start()
    {
        if (_recievedNode != null)
        {
            SetNode(_recievedNode);
            StartsConnected = true;
        }
        else StartsConnected = false;
    }
    public bool CanInteract(PlayerTDController player){ return player.HasNode() && _recievedNode == null; }

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
    private void SetNode(NodeController node)
    {
        node.Attach(_nodePos.localPosition, transform, Vector3.one * 0.15f);
        _recievedNode = node;
        OnNodeConnected?.Invoke(node.NodeType, true);
        
        if(_doorsView != null)
        {
            foreach (var item in _doorsView)
            {
                item.OpenDoor(true);
            }
        }
    }

    public void UnsetNode(NodeController node)
    {
        OnNodeConnected?.Invoke(_recievedNode.NodeType, false);
        _recievedNode = null;

        if (_doorsView != null)
        {
            foreach (var item in _doorsView)
            {
                item.OpenDoor(true);
            }
        }
    }
}