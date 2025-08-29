using System;
using UnityEngine;

public class GenericConnectionController : Connection<TaskManager>
{
    [SerializeField] private Transform _nodePos;

    // El TaskManager universal se suscribe a este evento
    public Action<NodeType, bool> OnNodeConnected;

    public override void SetSecTM(TaskManager _)
    {
        // No-op: el TaskManager central se suscribe a OnNodeConnected
    }

    public override bool CanInteract(PlayerTDController player)
    {
        return player.HasNode() && _recievedNode == null;
    }

    protected override void SetNode(NodeController node)
    {
        if (_nodePos != null)
            node.Attach(_nodePos.localPosition, transform, Vector3.one * 0.15f);
        else
            node.Attach(Vector3.zero, transform, Vector3.one * 0.15f);

        _recievedNode = node;
        OnNodeConnected?.Invoke(node.NodeType, true);
    }

    public override void UnsetNode(NodeController node = null)
    {
        if (_recievedNode == null) return;

        OnNodeConnected?.Invoke(_recievedNode.NodeType, false);
        _recievedNode = null;
    }

    public void EjectNode(Vector3 position = default)
    {
        if (_recievedNode == null) return;
        _recievedNode.transform.position = position;
    }
}
