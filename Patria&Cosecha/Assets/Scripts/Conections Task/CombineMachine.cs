using UnityEngine;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private ElectricityNode _combinedNode;
    [SerializeField] private Collider _trigger;
    [SerializeField] private Transform _firstNodePos;
    [SerializeField] private Transform _secondNodePos;
    [SerializeField] private Transform _combinedNodePos;

    private ElectricityNode _firstNode = default, _secondNode = default;
    private bool _isActive = false;

    public bool IsActive { get { return _isActive; } }

    private void Update()
    {
        if (_firstNode != null && _secondNode != null && IsValidCombination(_firstNode.NodeType, _secondNode.NodeType)) Activate();
        else Deactivate();
    }

    private void Activate()
    {
        _isActive = true;
        _trigger.enabled = false;
    }

    private void Deactivate()
    {
        _isActive = false;
        _trigger.enabled = true;
    }

    public void CombineNodes()
    {
        _combinedNode.Attach(transform, Vector3.zero);
        Instantiate(_combinedNode, _combinedNodePos);
        _combinedNode = null;
    }

    private bool IsValidCombination(NodeType firstType, NodeType secondType) => (firstType != NodeType.Dash || secondType != NodeType.Dash);

    public void SetNode(ElectricityNode node)
    {
        if (node == null) return;

        if (_firstNode == null)
        {
            node.Attach(transform, _firstNodePos.localPosition);
            _firstNode = node;
        }
        else if (_firstNode != null && node.NodeType != _firstNode.NodeType)
        {
            node.Attach(transform, _secondNodePos.localPosition);
            _secondNode = node;
        }
    }

    public void UnsetNode(ElectricityNode node)
    {
        if (_firstNode != null || _secondNode != null)
        {
            if (node.NodeType == _firstNode.NodeType) _firstNode = null;
            else if (node.NodeType == _secondNode.NodeType) _secondNode = null;
        }
    }
}
