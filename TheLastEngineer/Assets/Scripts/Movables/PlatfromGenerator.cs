using UnityEngine;

public class PlatfromGenerator : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection = default;
    [SerializeField] private Material _ActiveMat;
    [SerializeField] private Material _DisabledMat;

    private NodeType _requiredNode = NodeType.Green;
    private MeshRenderer _renderer = default;
    private BoxCollider _collider = default;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<BoxCollider>();

        _connection.OnNodeConnected += GeneratePlatform;
    }

    private void GeneratePlatform(NodeType node, bool isRunning)
    {
        if (node == _requiredNode && isRunning || node == NodeType.Dash && isRunning)
        {
            _renderer.material = _ActiveMat;
            _collider.enabled = true;
        }
        else
        {
            _renderer.material = _DisabledMat;
            _collider.enabled = false;
        }
    }
}
