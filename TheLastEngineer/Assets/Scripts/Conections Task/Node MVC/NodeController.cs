using UnityEngine;

public class NodeController : MonoBehaviour, IInteractable
{
    [Header("View")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private Collider _collider;
    
    [Header("For Player's Dash hability only<br>(if node is of <color=#14a3c7>BLUE</color> type set bool TRUE)")]
    [SerializeField] private bool _isDashNode = false;
    [SerializeField] private float _emissionIntensity = 4f;
    private PlayerTDController _player = null;


    [Header("Outline")]
    [SerializeField] private Outline _outline;
    [SerializeField] private Color _outlineColor;

    [Header("Model")]
    [SerializeField] private NodeType _nodeType;
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

    private void Awake()
    {
        _nodeModel = new NodeModel(transform, _minY, _maxY, _moveSpeed, _rotSpeed);
        _nodeView = new NodeView(_renderer, _material, _collider, _outline, _outlineColor, _emissionIntensity);
    }

    private void Start()
    {
        if (_isDashNode) _player = PlayerTDController.Instance;

        _nodeView.OnStart();
    }

    private void Update()
    {
        if (!_isChildren) MoveObject();
    }

    public bool CanInteract(PlayerTDController player)
    {
        return !player.HasNode();
    }

    public void Interact(PlayerTDController player, out bool succededInteraction)
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

    private void Attach(PlayerTDController player, Vector3 newPos)
    {
        if (_player != null && _nodeType == NodeType.Blue) _player.OnDash += UseHability;

        Vector3 newScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        if (_nodeType == NodeType.Dash) newScale = Vector3.one;
        
        Attach(newPos, player.transform, newScale, true);

        if (_isChildren && _connectable != null)
        {
            _connectable.UnsetNode(this);
            _connectable = null;
        }
    }

    public void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false)
    {
        if (parentIsPlayer) _nodeView.EnableColl(false);
        else
        {
            if (_player != null && _nodeType == NodeType.Blue) _player.OnDash -= UseHability;

            _nodeView.EnableColl(true);
        }

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

    public bool Combine(float deltaTime)
    {
        _nodeView.EnableColl(false);
        return _nodeModel.Combine(deltaTime);
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
