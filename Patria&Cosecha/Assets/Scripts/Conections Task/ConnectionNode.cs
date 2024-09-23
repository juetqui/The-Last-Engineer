using System.Collections;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private TaskManager _taskManager = default;
    [SerializeField] private ElectricityNode _cubeNode = default, _sphereNode = default, _capsuleNode = default;
    [SerializeField] private NodeType _requiredType = default;

    private ConnectionParticles _connectionPs;
    private MeshRenderer _renderer;
    private bool _isWorking = default, _hasError = default, _isDisabled = false;
    private NodeType _typeReceived = default;

    public bool IsWorking { get { return _isWorking; } }
    public bool HasError { get { return _hasError; } }

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        TurnOffNodes();
        _connectionPs = new ConnectionParticles(_particles);
    }

    void Update()
    {
        if (!_isDisabled) TurnOnReceivedNode();
        _connectionPs.OnUpdate();
    }

    private void TurnOnReceivedNode()
    {
        _renderer.enabled = false;

        if (_typeReceived == NodeType.Cube)
        {
            _cubeNode.gameObject.SetActive(true);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(false);
        }
        else if (_typeReceived == NodeType.Sphere)
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(true);
            _capsuleNode.gameObject.SetActive(false);
        }
        else if (_typeReceived == NodeType.Capsule)
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(true);
        }
        else
        {
            _renderer.enabled = true;
            TurnOffNodes();
        }
    }

    private void TurnOffNodes()
    {
        _cubeNode.gameObject.SetActive(false);
        _sphereNode.gameObject.SetActive(false);
        _capsuleNode.gameObject.SetActive(false);
    }

    private void CheckReceivedNode()
    {
        if (_typeReceived == _requiredType)
        {
            _isWorking = true;
            _hasError = false;
            _taskManager.AddConnection(_typeReceived);
        }
        else
        {
            _taskManager.RemoveConnection(_requiredType);
            _isWorking = false;
            _hasError = true;
            StartCoroutine(DisableConnection());
        }
    }

    public void SetNode(NodeType node)
    {
        _typeReceived = node;
        CheckReceivedNode();
    }

    private IEnumerator DisableConnection()
    {
        _isDisabled = true;
        Color oldColor = _renderer.material.color;
        _renderer.material.color = Color.red;

        yield return new WaitForSeconds(3f);

        _isDisabled = false;
        _renderer.material.color = oldColor;
    }
}

public enum NodeType
{
    None,
    Cube,
    Sphere,
    Capsule
}
