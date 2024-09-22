using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [SerializeField] private NodeType _nodeType = default;

    public NodeType NodeType { get { return _nodeType; } }
}
