using System.Collections;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    [SerializeField] private TaskManager[] _taskManagers;
    [SerializeField] private NodeType _requiredType;

    [Header("MVC View")]
    [SerializeField] private Renderer _render;
    [SerializeField] private Collider _colider;
    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private AudioSource _audioSrc;
    [SerializeField] private AudioClip _placedClip;
    [SerializeField] private AudioClip _errorClip;
    [SerializeField] private Color _color;
    [SerializeField] private Color _secColor;

    [ColorUsage(true, true)]
    [SerializeField] private Color _fresnelColor;

    [SerializeField] private ParticleSystem _ps;


    private NodeRenderer _nodeRenderer = default;
    private NodeChecker _nodeChecker = default;
    private ElectricityNode _recievedNode = default;

    private bool _isDisabled = false;

    public bool IsDisabled { get { return _isDisabled; } }

    private void Start()
    {
        _nodeRenderer = new NodeRenderer(_requiredType, _render, _colider, _triggerCollider, _color, _secColor, _fresnelColor, _ps, _audioSrc);
        _nodeRenderer.OnStart();
        _nodeChecker = new NodeChecker(_taskManagers, _requiredType);
    }

    public void SetNode(ElectricityNode node)
    {
        if (_isDisabled || node.NodeType == NodeType.None) return;

        node.Attach(Vector3.zero, transform);
        _recievedNode = node;
        CheckReceivedNode();
    }

    public void UnsetNode()
    {
        _recievedNode = null;
        _nodeRenderer.Enable(true);
        _nodeRenderer.EnableTrigger(true);

        foreach (var tm in _taskManagers)
            tm.RemoveConnection(_requiredType);
    }

    private void CheckReceivedNode()
    {
        _nodeRenderer.EnableTrigger(false);

        if (_nodeChecker.IsNodeCorrect(_recievedNode.NodeType))
        {
            _recievedNode.IsConnected = true;
            _nodeRenderer.Enable(false);
            _nodeRenderer.PlayClip(_placedClip);
            _nodeRenderer.PlayEffect(false);
            _nodeChecker.HandleNodeCorrect(_recievedNode.NodeType);
        }
        else
        {
            _recievedNode.IsConnected = false;
            _nodeRenderer.Enable(true);
            _nodeRenderer.PlayClip(_errorClip);
            _nodeRenderer.PlayEffect(true);
            _nodeChecker.HandleNodeIncorrect();
            StartCoroutine(DisableConnection());
        }
    }

    private IEnumerator DisableConnection()
    {
        _isDisabled = true;
        _nodeRenderer.ChangeColor(Color.red);

        yield return new WaitForSeconds(3f);

        _isDisabled = false;
        _nodeRenderer.ChangeColor(_color);
    }
}
