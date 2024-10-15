using System.Collections;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    private NodeRenderer _nodeRenderer;
    private NodeChecker _nodeChecker;

    [SerializeField] private TaskManager _taskManager;
    [SerializeField] private NodeType _requiredType;

    [SerializeField] private NodeType _typeReceived = NodeType.None;
    private bool _isDisabled = false;

    [SerializeField] private Renderer _nodeRendererRender;
    [SerializeField] private Collider _nodeRendererColider;
    [SerializeField] private AudioSource _audioSrc = default;
    [SerializeField] private AudioClip _placedClip = default;
    [SerializeField] private AudioClip _errorClip = default;
    [SerializeField] private Color _defaultColor = default;
    [SerializeField] private ParticleSystem _ps = default;

    private void Start()
    {
        _nodeRenderer = new NodeRenderer(_nodeRendererRender, _nodeRendererColider, _defaultColor, _ps, _audioSrc);
        _nodeRenderer.OnStart();
        _nodeChecker = new NodeChecker(_taskManager, _requiredType);
    }

    public void SetNode(NodeType node)
    {
        if (!_isDisabled && node != NodeType.None)
        {
            _typeReceived = node;
            CheckReceivedNode();
        }
    }

    private void CheckReceivedNode()
    {
        if (_nodeChecker.IsNodeCorrect(_typeReceived))
        {
            _nodeRenderer.Enable(false);
            _nodeRenderer.PlayClip(_placedClip);
            _nodeRenderer.PlayEffect(false);
            _nodeChecker.HandleNodeCorrect(_typeReceived);
        }
        else
        {
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
    }
}
