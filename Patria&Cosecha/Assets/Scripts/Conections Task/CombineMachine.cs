using UnityEngine;
using UnityEngine.Assertions.Must;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private ElectricityNode _combinedNode;
    [SerializeField] private Vector3 _firstNodePos, _secondNodePos;

    private ElectricityNode _firstNode = default, _secondNode = default;

    void Start()
    {
    }

    void Update()
    {
        if (_firstNode != null && _secondNode != null && IsValidCombination(_firstNode.NodeType, _secondNode.NodeType)) CombineNodes();
    }

    private void CombineNodes()
    {
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
