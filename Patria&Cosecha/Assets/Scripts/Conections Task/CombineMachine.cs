using UnityEngine;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private ElectricityNode[] _firstNodePrefabs = default;
    [SerializeField] private ElectricityNode[] _secondNodePrefabs = default;
    [SerializeField] private CombinedNode[] _combinedNodes = default;

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
        if (_firstNode == NodeType.Cube)
        {
            if (_secondNode == NodeType.Sphere)
            {

            }
            else if (_secondNode == NodeType.Capsule)
            {
                foreach (var combined in _combinedNodes)
                {
                    if (combined.NodeType == NodeType.CubeCapsule) combined.gameObject.SetActive(true);
                }
            }
        }
        else if (_firstNode == NodeType.Sphere)
        {
            if (_secondNode == NodeType.Cube)
            {

            }
            else if (_secondNode == NodeType.Capsule)
            {

            }
        }
        else if (_firstNode == NodeType.Capsule)
        {
            if (_secondNode == NodeType.Cube)
            {
                foreach (var combined in _combinedNodes)
                {
                    if (combined.NodeType == NodeType.CubeCapsule) combined.gameObject.SetActive(true);
                }
            }
            else if (_secondNode == NodeType.Sphere)
            {

            }
        }
    }

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
        foreach (var firstPrefab in _firstNodePrefabs)
        {
            firstPrefab.gameObject.SetActive(false);
        }

        foreach (var secondPrefab in _secondNodePrefabs)
        {
            secondPrefab.gameObject.SetActive(false);
        }

        foreach (var combinedNode in _combinedNodes)
        {
            combinedNode.gameObject.SetActive(false);
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
