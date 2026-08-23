using System;
using UnityEngine;

public class Connection : MonoBehaviour, IInteractable, IConnectable, IProximityListener
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

    public NodeType RequiredType {  get { return _requiredType; } }
    public bool StartsConnected { get; private set; }
    public bool IsConnected => _recievedNode != null;

    public Action OnInitialized;
    public Action<NodeType, bool> OnNodeConnected;
    public Action<bool> OnAvailableToConnect;

    private void Start()
    {
        //_particleNode.SetActive(true);
        _renderer = GetComponentInParent<Renderer>();
        _renderer.material.SetColor("_EmissiveColor", _emissionOff);
        OnInitialized?.Invoke();

        if (_recievedNode != null)
        {
            SetNode(_recievedNode);
            StartsConnected = true;
        }
        else StartsConnected = false;
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler.HasNode && _recievedNode == null;

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
        _renderer.material.SetColor("_EmissiveColor", _emissionOff);
        _recievedNode = null;
        //_particleNode.SetActive(true);
    }

    public void OnPlayerProximity(bool inRange, PlayerController player)
    {
        if (inRange)
        {
            if (player.NodeHandler.CurrentType == _requiredType && !IsConnected)
                OnAvailableToConnect?.Invoke(true);
        }
        else
        {
            OnAvailableToConnect?.Invoke(false);
        }
    }
}