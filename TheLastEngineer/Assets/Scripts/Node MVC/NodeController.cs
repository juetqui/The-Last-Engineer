using System;
using UnityEngine;

public class NodeController : MonoBehaviour, IInteractable
{
    [Header("View")]
    private Renderer _renderer = default;
    private Collider _collider = default;
    protected Animator _animator = default;

    [Header("Outline")]
    [SerializeField] private Color _corruptionOutline;
    [SerializeField] private Color _defaultOutline;
    private Color _currentOutline = default;
    private Outline _outline = default;

    [Header("Model")]
    [SerializeField] protected NodeType _nodeType;
    [SerializeField] private float _rotSpeed, _minY, _maxY, _moveSpeed;
    [SerializeField] private bool _isChildren;

    [SerializeField] private bool _debug = false; 

    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => true;

    private NodeView _nodeView = default;
    private NodeModel _nodeModel = default;

    private IConnectable _connectable = default;
    
    private bool _isConnected = false;

    public Action<NodeType> OnUpdatedNodeType = delegate { };

    public NodeType NodeType { get { return _nodeType; } }
    public Color OutlineColor { get { return _currentOutline; } }
    public bool IsChildren { get { return _isChildren; } }
    public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }

    protected void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _outline = GetComponentInChildren<Outline>();
        
        _currentOutline = _nodeType == NodeType.Default ? _defaultOutline : _corruptionOutline;

        _nodeModel = new NodeModel(transform, _minY, _maxY, _moveSpeed, _rotSpeed);
        _nodeView = new NodeView(_renderer, _collider, _outline, _currentOutline, _animator);
    }

    protected void Start()
    {
        _nodeView.OnStart();
    }

    protected void Update()
    {
        if (!_isChildren) MoveObject();
        else _nodeView.SetCollectedAnim();
    }

    public bool CanInteract(PlayerTDController player)
    {
        return !player.HasNode();
    }

    private void InteractWithGlitcheable(Glitcheable glitcheable)
    {
        if (glitcheable == null) return;

        bool newObjectState = _nodeType == NodeType.Default ? false : true;

        if (glitcheable.ChangeCorruptionState(_nodeType, newObjectState))
            UpdateNodeType();
    }

    private void UpdateNodeType()
    {
        _nodeType = _nodeType == NodeType.Default ? NodeType.Corrupted : NodeType.Default;
        _currentOutline = _nodeType == NodeType.Default ? _defaultOutline : _corruptionOutline;

        _nodeView.UpdateNodeType(_nodeType, _currentOutline);
        OnUpdatedNodeType?.Invoke(_nodeType);
    }

    public  void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (CanInteract(player))
        {
            Attach(player, player.attachPos);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }
    }

    private void MoveObject()
    {
        _nodeView.EnableColl(true);
        _nodeModel.MoveObject();
    }

    protected void Attach(PlayerTDController player, Vector3 newPos)
    {
        Vector3 newScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        Attach(newPos, player.transform, newScale, true);

        if (_isChildren && _connectable != null)
        {
            _connectable.UnsetNode(this);
            _connectable = null;
        }
    }

    public void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false)
    {
        if (parentIsPlayer)
        {
            GlitchActive.Instance.OnChangeObjectState += InteractWithGlitcheable;
            _nodeView.EnableColl(false);
        }
        else
        {
            GlitchActive.Instance.OnChangeObjectState -= InteractWithGlitcheable;
            _nodeView.EnableColl(true);
        }

        if (!parentIsPlayer && newParent != null)
            _connectable = newParent.GetComponent<IConnectable>();

        if (newParent != null) _isChildren = true;
        else _isChildren = false;

        if (newParent != null && newScale != default)
            _nodeModel.SetPos(newPos, NodeType, newParent, newScale);
        else if (newParent != null && newScale == default)
            _nodeModel.SetPos(newPos, NodeType, newParent);
        else if (newParent == null && newScale == default)
            _nodeModel.SetPos(newPos, NodeType);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerTDController>())
        {
            _nodeView.SetRangeAnim();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerTDController>())
        {
            _nodeView.SetIdleAnim();
        }
    }
}
