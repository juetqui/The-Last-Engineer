using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private Collider _collider;
    
    [Header("Emissive")]
    [SerializeField] PlayerTDController _player;
    [SerializeField] private float _resetTime;

    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _unaviable;

    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _aviable;


    [Header("Outline")]
    [SerializeField] private Outline _outline;
    [SerializeField] private Color _outlineColor;

    [Header("Model")]
    [SerializeField] private NodeType _nodeType;
    [SerializeField] private float _rotSpeed, _minY, _maxY, _moveSpeed;
    [SerializeField] private bool _isChildren = false;

    private NodeView _nodeView = default;
    private NodeModel _nodeModel = default;

    private ConnectionNode _connectionNode = default;
    private CombineMachine _combineMachine = default;
    
    private bool _isConnected = false;

    public NodeType NodeType { get { return _nodeType; } }
    public Color OutlineColor { get { return _outlineColor; } }
    public bool IsChildren { get { return _isChildren; } }
    public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }

    private void Awake()
    {
        _nodeModel = new NodeModel(transform, _minY, _maxY, _moveSpeed, _rotSpeed);
        _nodeView = new NodeView(_renderer, _material, _collider, _outline, _outlineColor, _unaviable, _aviable, _resetTime);
    }

    private void Start()
    {
        _nodeView.OnStart();

        if (_player != null && _nodeType == NodeType.Blue) _player.onDash += UseHability;
    }

    private void Update()
    {
        if (!_isChildren) MoveObject();
    }

    private void MoveObject()
    {
        _nodeView.EnableColl(true);
        _nodeModel.MoveObject(Time.deltaTime);
    }

    public void Attach(PlayerTDController player, Vector3 newPos)
    {
        Vector3 newScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        if (_nodeType == NodeType.Dash) newScale = Vector3.one;
        
        Attach(newPos, player.transform, newScale, true);

        if (_isChildren && _connectionNode != null)
        {
            _connectionNode.UnsetNode();
            _connectionNode = null;
        }
        else if (_isChildren && _combineMachine != null)
        {
            _combineMachine.UnsetNode(this);
            _combineMachine = null;
        }
    }

    public void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false)
    {
        if (parentIsPlayer) _nodeView.EnableColl(false);
        else _nodeView.EnableColl(true);

        if (!parentIsPlayer && newParent != null)
        {
            _connectionNode = newParent.GetComponent<ConnectionNode>();
            _combineMachine = newParent.GetComponent<CombineMachine>();
        }

        if (newParent != null) _isChildren = true;
        else _isChildren = false;

        if (newParent != null && newScale != default) _nodeModel.SetPos(newPos, NodeType, newParent, newScale);
        else if (newParent != null && newScale == default) _nodeModel.SetPos(newPos, NodeType, newParent);
        else if (newParent == null && newScale == default) _nodeModel.SetPos(newPos, NodeType);
    }

    public bool Combine(float deltaTime)
    {
        _nodeView.EnableColl(false);
        return _nodeModel.Combine(deltaTime);
    }

    public void UseHability()
    {
        StartCoroutine(_nodeView.ResetHability());
    }
}
