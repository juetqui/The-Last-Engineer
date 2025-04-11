using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericConnectionController : Connection<SecTM>
{
    private List<SecTM> _secTaskManagers = new List<SecTM>();
    
    private NodeController _recievedNode = default;

    public Action<NodeType, bool> OnNodeConnected;

    public override void SetSecTM(SecTM secTM)
    {
        _secTaskManagers.Add(secTM);
    }

    public override bool CanInteract(PlayerTDController player)
    {
        return player.HasNode();
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
}
