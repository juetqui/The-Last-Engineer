using System.Collections;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    [SerializeField] private Collider _collider = default;
    [SerializeField] private AudioSource _audioSrc = default;
    [SerializeField] private TaskManager _taskManager = default;
    [SerializeField] private ParticleSystem _particles = default;
    [SerializeField] private AudioClip _placedClip = default, _errorClip = default;
    [SerializeField] private ElectricityNode _cubeNode = default, _sphereNode = default, _capsuleNode = default;
    
    [SerializeField] private Color _color = default;
    [SerializeField] private NodeType _requiredType = default;

    private ConnectionParticles _connectionPs = default;
    private ConnectionsView _connectionsView = default;
    private MeshRenderer _renderer = default;
    private bool _isWorking = default, _hasError = default, _isDisabled = false;
    private NodeType _typeReceived = default;

    public bool IsWorking { get { return _isWorking; } }
    public bool HasError { get { return _hasError; } }

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        TurnOffNodes();
        _connectionPs = new ConnectionParticles(_particles);

        _connectionsView = new ConnectionsView(_renderer.material, _color);
        _connectionsView.OnStart();
    }

    void Update()
    {
        if (!_isDisabled) TurnOnReceivedNode();
    }

    private void TurnOnReceivedNode()
    {
        if (_typeReceived == NodeType.Cube)
        {
            TurnOffNodes();
            _cubeNode.gameObject.SetActive(true);
        }
        else if (_typeReceived == NodeType.Sphere)
        {
            TurnOffNodes();
            _sphereNode.gameObject.SetActive(true);
        }
        else if (_typeReceived == NodeType.Capsule)
        {
            TurnOffNodes();
            _capsuleNode.gameObject.SetActive(true);
        }
        else TurnOffNodes();
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
            _audioSrc.clip = _placedClip;
            _connectionPs.ActivatePSError(false);
            _renderer.enabled = false;
            _collider.enabled = false;
            _isWorking = true;
            _hasError = false;
            _taskManager.AddConnection(_typeReceived);
        }
        else
        {
            _audioSrc.clip = _errorClip;
            _renderer.enabled = true;
            _collider.enabled = true;
            _taskManager.RemoveConnection(_requiredType);
            _isWorking = false;
            _hasError = true;

            _connectionPs.ActivatePSError(true);
            StartCoroutine(DisableConnection());
        }

        TurnOnReceivedNode();
        _audioSrc.Play();
    }

    public void SetNode(NodeType node)
    {
        if (!_isDisabled && node != NodeType.None)
        {
            _typeReceived = node;
            CheckReceivedNode();
        }
    }

    public void ResetNode()
    {
        _typeReceived = NodeType.None;
        CheckReceivedNode();
    }

    private IEnumerator DisableConnection()
    {
        _isDisabled = true;
        _renderer.material.color = Color.red;

        yield return new WaitForSeconds(3f);

        _isDisabled = false;
        _renderer.material.color = _color;
    }
}
