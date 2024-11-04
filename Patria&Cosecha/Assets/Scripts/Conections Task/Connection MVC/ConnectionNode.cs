using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    [SerializeField] private NodeType _requiredType;

    [Header("MVC View")]
    [SerializeField] private Renderer _render;
    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private AudioSource _audioSrc;
    [SerializeField] private AudioClip _placedClip;
    [SerializeField] private AudioClip _errorClip;
    [SerializeField] private Color _color;
    [SerializeField] private Color _secColor;

    [ColorUsage(true, true)]
    [SerializeField] private Color _fresnelColor;

    [SerializeField] private ParticleSystem _ps;

    private MainTM _mainTM = default;
    private List<SecondaryTM> _secTaskManagers = new List<SecondaryTM>();
    private NodeRenderer _nodeRenderer = default;
    private ElectricityNode _recievedNode = default;

    private bool _isDisabled = false, _isWorking = false;

    public bool IsDisabled { get { return _isDisabled; } }
    public bool IsWorking { get { return _isWorking; } }

    private void Awake()
    {
        _nodeRenderer = new NodeRenderer(_requiredType, _render, _triggerCollider, _color, _secColor, _fresnelColor, _ps, _audioSrc);
        _nodeRenderer.OnStart();
    }

    public void SetMainTM(MainTM taskManager)
    {
        _mainTM = taskManager;
    }

    public void SetSecTM(SecondaryTM secTM)
    {
        _secTaskManagers.Add(secTM);
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
        HandleTaskManagers(false);
    }

    private void CheckReceivedNode()
    {
        _nodeRenderer.EnableTrigger(false);

        if (_recievedNode.NodeType == _requiredType) HandleRecievedNode(true, false, _placedClip);
        else
        {
            HandleRecievedNode(false, true, _errorClip);
            StartCoroutine(DisableConnection());
        }
    }

    private void HandleRecievedNode(bool isValid, bool playEffects, AudioClip clip)
    {
        if (isValid) HandleTaskManagers(isValid);

        _isWorking = isValid;
        _recievedNode.IsConnected = isValid;
        _nodeRenderer.Enable(playEffects);
        _nodeRenderer.PlayEffect(playEffects);
        _nodeRenderer.PlayClip(_placedClip);
    }

    private void HandleTaskManagers(bool addConnection)
    {
        if (addConnection)
        {
            Debug.Log("Add");
            if (_mainTM != null) _mainTM.AddConnection(_requiredType);

            if (_secTaskManagers.Count > 0)
            {
                Debug.Log("Add SecTM");
                foreach (var secTM in _secTaskManagers) secTM.AddConnection(_requiredType);
            }
        }
        else
        {
            Debug.Log("Remove");
            if (_mainTM != null) _mainTM.RemoveConnection(_requiredType);

            if (_secTaskManagers.Count > 0)
            {
                foreach (var secTM in _secTaskManagers) secTM.RemoveConnection(_requiredType);
            }
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
