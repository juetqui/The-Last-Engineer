using UnityEngine;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private ElectricityNode _combinedNode;
    [SerializeField] private Collider _trigger;
    [SerializeField] private Vector3 _firstNodePos;
    [SerializeField] private Vector3 _secondNodePos;

    private ElectricityNode _firstNode = default, _secondNode = default;

    void Start()
    {
    }

    void Update()
    {
        if (_firstNode != null && _secondNode != null && IsValidCombination(_firstNode.NodeType, _secondNode.NodeType)) CombineNodes();
        else _trigger.enabled = true;
    }

    private void CombineNodes()
    {
        _trigger.enabled = false;
        Debug.Log("Combine");
        // SI EL PLAYER ATIVA LA MÁQUINA, SE CREA EL NODO COMBINADO CORRSPONDIENTE
    }

    private bool IsValidCombination(NodeType firstType, NodeType secondType) => (firstType != NodeType.Dash || secondType != NodeType.Dash);

    public void SetNode(ElectricityNode node)
    {
        if (node.NodeType == NodeType.None) return;
        else if (_firstNode == null)
        {
            _firstNode = node;
            node.Attach(transform, _firstNodePos);
        }
        else if (_firstNode != null && node.NodeType != _firstNode.NodeType)
        {
            _secondNode = node;
            node.Attach(transform, _secondNodePos);
        }
        else return;
    }

    public void UnsetNode(ElectricityNode node)
    {
        if (node.NodeType == _firstNode.NodeType) _firstNode = null;
        else if (node.NodeType == _secondNode.NodeType) _secondNode = null;
        else return;
    }
}
