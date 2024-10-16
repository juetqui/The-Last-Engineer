using System.Collections;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    [SerializeField] private TaskManager _taskManager = default;
    [SerializeField] private NodeType _requiredType = default;

    [Header("MVC View")]
    [SerializeField] private Renderer _nodeRendererRender = default;
    [SerializeField] private Collider _nodeRendererColider = default;
    [SerializeField] private AudioSource _audioSrc = default;
    [SerializeField] private AudioClip _placedClip = default;
    [SerializeField] private AudioClip _errorClip = default;
    [SerializeField] private Color _defaultColor = default;
    [SerializeField] private ParticleSystem _ps = default;


    private NodeRenderer _nodeRenderer = default;
    private NodeChecker _nodeChecker = default;
    private ElectricityNode _recievedNode = default;

    private NodeType _typeReceived = NodeType.None;
    private bool _isDisabled = false;

    private void Start()
    {
        _nodeRenderer = new NodeRenderer(_nodeRendererRender, _nodeRendererColider, _defaultColor, _ps, _audioSrc);
        _nodeRenderer.OnStart();
        _nodeChecker = new NodeChecker(_taskManager, _requiredType);
    }

    public void SetNode(ElectricityNode node)
    {
        Debug.Log(node);
        if (!_isDisabled && node.NodeType != NodeType.None)
        {
            node.Attach(transform, Vector3.zero);
            _recievedNode = node;
            _typeReceived = node.NodeType;
            CheckReceivedNode();
        }
    }

    private void CheckReceivedNode()
    {
        if (_nodeChecker.IsNodeCorrect(_typeReceived))
        {
            _recievedNode.IsConnected = true;
            _nodeRenderer.Enable(false);
            _nodeRenderer.PlayClip(_placedClip);
            _nodeRenderer.PlayEffect(false);
            _nodeChecker.HandleNodeCorrect(_typeReceived);
        }
        else
        {
            _recievedNode.IsConnected = false;
            _nodeRenderer.Enable(true);
            _nodeRenderer.PlayClip(_errorClip);
            _nodeRenderer.PlayEffect(true);
            _nodeChecker.HandleNodeIncorrect(_typeReceived);
            StartCoroutine(DisableConnection());
        }
    }

    private IEnumerator DisableConnection()
    {
        _isDisabled = true;
        _nodeRenderer.ChangeColor(Color.red);

        yield return new WaitForSeconds(3f);

        _isDisabled = false;
        _nodeRenderer.ChangeColor(_defaultColor);
        _recievedNode = null;
        _typeReceived = NodeType.None;
    }
}
