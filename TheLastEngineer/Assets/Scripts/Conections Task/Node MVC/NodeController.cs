using UnityEngine;

public abstract class NodeController : MonoBehaviour, IInteractable
{
    [Header("View")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private Collider _collider;
    
    [Header("For Player's Dash hability only<br>(if this script is <color=#14a3c7>MaterializerNode</color> set value greater than 0)")]
    [SerializeField] protected float _emissionIntensity = 4f;


    [Header("Outline")]
    [SerializeField] private Outline _outline;
    [SerializeField] private Color _outlineColor;

    [Header("Model")]
    [SerializeField] protected NodeType _nodeType;
    [SerializeField] private float _rotSpeed, _minY, _maxY, _moveSpeed;
    [SerializeField] private bool _isChildren = false;

    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;

    private NodeView _nodeView = default;
    private NodeModel _nodeModel = default;

    private IConnectable _connectable = default;
    
    private bool _isConnected = false;

    public NodeType NodeType { get { return _nodeType; } }
    public Color OutlineColor { get { return _outlineColor; } }
    public bool IsChildren { get { return _isChildren; } }
    public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }

    protected void OnAwake()
    {
        _nodeModel = new NodeModel(transform, _minY, _maxY, _moveSpeed, _rotSpeed);
        _nodeView = new NodeView(_renderer, _material, _collider, _outline, _outlineColor, _emissionIntensity);
    }

    protected void OnStart()
    {
        _nodeView.OnStart();
    }

    protected void OnUpdate()
    {
        if (!_isChildren) MoveObject();
    }

    public bool CanInteract(PlayerTDController player)
    {
        return !player.HasNode();
    }

    public virtual void Interact(PlayerTDController player, out bool succededInteraction)
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

    protected virtual void Attach(PlayerTDController player, Vector3 newPos)
    {
        Vector3 newScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        Attach(newPos, player.transform, newScale, true);

        if (_isChildren && _connectable != null)
        {
            _connectable.UnsetNode(this);
            _connectable = null;
        }
    }

    public virtual void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false)
    {
        if (parentIsPlayer) _nodeView.EnableColl(false);
        else _nodeView.EnableColl(true);

        if (!parentIsPlayer && newParent != null)
        {
            _connectable = newParent.GetComponent<IConnectable>();
        }

        if (newParent != null) _isChildren = true;
        else _isChildren = false;

        if (newParent != null && newScale != default)
        {
            _nodeModel.SetPos(newPos, NodeType, newParent, newScale);
        }
        else if (newParent != null && newScale == default)
        {
            _nodeModel.SetPos(newPos, NodeType, newParent);
        }
        else if (newParent == null && newScale == default)
        {
            _nodeModel.SetPos(newPos, NodeType);
        }
    }

    public void UseHability(float dashDuration, float dashCD)
    {
        StartCoroutine(_nodeView.ResetHability(dashDuration, dashCD));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * 5f);
    }
}
