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
    [SerializeField] private GameObject _particleNode;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionDefault;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionCorrupted;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionCorrect;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionIncorrect;

    private string _emissiveColor = "_EmissiveColor";
    private Color _emissionOff;
    private Renderer _renderer = default;

    public NodeType RequiredType {  get { return _requiredType; } }
    public bool StartsConnected { get; private set; }
    public bool IsConnected => _recievedNode != null;

    public Action OnInitialized;
    public Action<NodeType, bool> OnNodeConnected;

    private void Start()
    {
        _particleNode.SetActive(true);
        _renderer = GetComponent<Renderer>();
        _emissionOff = _requiredType == NodeType.Corrupted ? _emissionCorrupted : _emissionDefault;
        _renderer.material.SetColor(_emissiveColor, _emissionOff);

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
            _renderer.material.SetColor(_emissiveColor, _emissionCorrect);
            _particleNode.SetActive(false);
        }
        else
        {
            _renderer.material.SetColor(_emissiveColor, _emissionIncorrect);
        }

    }

    public void UnsetNode(NodeController node)
    {
        print("unset");
        OnNodeConnected?.Invoke(_recievedNode.NodeType, false);
        _renderer.material.SetColor(_emissiveColor, _emissionOff);
        _recievedNode = null;
        _particleNode.SetActive(true);
    }
}