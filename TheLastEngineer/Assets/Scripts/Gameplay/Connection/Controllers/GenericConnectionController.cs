using System;
using UnityEngine;

public class GenericConnectionController : Connection<TaskManager>
{
    [SerializeField] private Transform _nodePos;

    public Action<NodeType, bool> OnNodeConnected;

    public override void SetSecTM(TaskManager _) { }

    public override bool CanInteract(PlayerTDController player) =>
        player.HasNode() && _recievedNode == null;

    protected override void SetNode(NodeController node)
    {
        Vector3 position = _nodePos != null ? _nodePos.localPosition : Vector3.zero;
        node.Attach(position, transform, Vector3.one * 0.15f);
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
