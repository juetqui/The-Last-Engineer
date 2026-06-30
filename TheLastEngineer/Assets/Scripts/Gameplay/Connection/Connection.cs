using System;
using UnityEngine;

public class Connection : MonoBehaviour, IInteractable, IConnectable
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.Medium;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    [SerializeField] private NodeController _recievedNode;
    [SerializeField] private Transform _nodePos;
    [SerializeField] private NodeType _requiredType = NodeType.Default;
    //[SerializeField] private GameObject _particleNode;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionOff;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionCorrect;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionIncorrect;
    
    [SerializeField] private float connectDelay = 0.25f;

    private Renderer _renderer = default;
    
    private float _timer = 0f;
    private bool _canConnect = true;

    public NodeType RequiredType {  get { return _requiredType; } }
    public bool StartsConnected { get; private set; }
    public bool IsConnected => _recievedNode != null;

    public Action OnInitialized;
    public Action<NodeType, bool> OnNodeConnected;
    public Action<bool> OnAvailableToConnect;

    private void Start()
    {
        //_particleNode.SetActive(true);
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetColor("_EmissiveColor", _emissionOff);
        OnInitialized?.Invoke();

        if (_recievedNode != null)
        {
            SetNode(_recievedNode);
            StartsConnected = true;
        }
        else StartsConnected = false;
    }

    private void Update()
    {
        UpdateConnectionDelay();
    }

    private void UpdateConnectionDelay()
    {
        if (_canConnect) return;

        if (_timer < connectDelay)
        {
            _timer += Time.deltaTime;
            return;
        }

        _canConnect = true;
        _timer = 0f;
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler.HasNode && _recievedNode == null && _canConnect;

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        if (_recievedNode != null)
        {
            succededInteraction = false;
        }
        else if (CanInteract(playerNodeHandler))
        {
            NodeController node = playerNodeHandler.CurrentNode;
            SetNode(node);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }
    }

    private void SetNode(NodeController node)
    {
        node.Attach(_nodePos.localPosition, transform, Vector3.one * 0.15f, false, _nodePos.rotation);
        _recievedNode = node;

        if (_recievedNode.NodeType == _requiredType)
        {
            OnNodeConnected?.Invoke(node.NodeType, true);
            _canConnect = false;
            _renderer.material.SetColor("_EmissiveColor", _emissionCorrect);
            //_particleNode.SetActive(false);
        }
        else
        {
            _renderer.material.SetColor("_EmissiveColor", _emissionIncorrect);
        }

    }

    public void UnsetNode(NodeController node)
    {
        OnNodeConnected?.Invoke(_recievedNode.NodeType, false);
        _canConnect = false;
        _renderer.material.SetColor("_EmissiveColor", _emissionOff);
        _recievedNode = null;
        //_particleNode.SetActive(true);
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerNodeHandler player) && player.CurrentType == _requiredType && !IsConnected)
            OnAvailableToConnect?.Invoke(true);
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerNodeHandler player))
            OnAvailableToConnect?.Invoke(false);
    }
}