using UnityEngine;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private ElectricityNode[] _firstNodePrefabs = default;
    [SerializeField] private ElectricityNode[] _secondNodePrefabs = default;
    //[SerializeField] private CombinedNode[] _combinedNodes = default;

    private NodeType _firstNode = NodeType.None, _secondNode = NodeType.None;

    void Start()
    {
        TurnOffNodes();
    }

    void Update()
    {
        if (_firstNode != NodeType.None && _secondNode != NodeType.None) CombineNodes();
    }

    private void CombineNodes()
    {
        if (_firstNode == NodeType.None || _secondNode == NodeType.None) return;

        //if (IsValidCombination(NodeType.Cube, NodeType.Capsule)) ActivateCombinedNode(NodeType.Dash);
        // A�adir otros casos seg�n sea necesario
    }

    private bool IsValidCombination(NodeType node1, NodeType node2)
    {
        return (_firstNode == node1 && _secondNode == node2) || (_firstNode == node2 && _secondNode == node1);
    }

    //private void ActivateCombinedNode(NodeType combinedType)
    //{
    //    foreach (var combined in _combinedNodes)
    //    {
    //        if (combined.NodeType == combinedType) combined.gameObject.SetActive(true);
    //    }
    //}

    private void TurnOnRecievedNode()
    {
        TurnOffNodes();
        
        if (_firstNode != NodeType.None)
        {
            foreach (var firstPrefab in _firstNodePrefabs)
            {
                if (firstPrefab.NodeType == _firstNode) firstPrefab.gameObject.SetActive(true);
            }
        }

        if (_secondNode != NodeType.None)
        {
            foreach (var secondPrefab in _secondNodePrefabs)
            {
                if (secondPrefab.NodeType == _secondNode) secondPrefab.gameObject.SetActive(true);
            }
        }

    }

    private void TurnOffNodes()
    {
        DisableAllNodes(_firstNodePrefabs);
        DisableAllNodes(_secondNodePrefabs);
        //DisableAllNodes(_combinedNodes);
    }

    private void DisableAllNodes(ElectricityNode[] nodes)
    {
        foreach (var node in nodes)
        {
            node.gameObject.SetActive(false);
        }
    }

    public void SetNode(NodeType node)
    {
        if (node == NodeType.None) return;

        if (_firstNode != NodeType.None && node != _firstNode) _secondNode = node;
        else _firstNode = node;
        TurnOnRecievedNode();
    }
}
