using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericConnectionController : Connection<GenericTM>
{
    private List<GenericTM> _secTaskManagers = new List<GenericTM>();
    public Action<NodeType, bool> OnNodeConnected;

    public override void SetSecTM(GenericTM secTM)
    {
        _secTaskManagers.Add(secTM);
    }

    public override bool CanInteract(PlayerTDController player)
    {
        return player.HasNode() && _recievedNode == null;
    }

    protected override void SetNode(NodeController node)
    {
        node.Attach(Vector3.zero, transform);
        _recievedNode = node;
        OnNodeConnected?.Invoke(node.NodeType, true);
    }

    public override void UnsetNode(NodeController node = null)
    {
        OnNodeConnected?.Invoke(_recievedNode.NodeType, false);
        _recievedNode = null;
    }
    public void EjectNode(Vector3 position = default)
    {
        _recievedNode.gameObject.transform.position = position;

    }
}
