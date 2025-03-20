using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private Collider _collider;
    
    [Header("Emissive (For Player's Dash Hability Only)")]
    [SerializeField] private PlayerTDController _player;
    [SerializeField] private float _emissionIntensity = 4f;


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
        _nodeView = new NodeView(_renderer, _material, _collider, _outline, _outlineColor, _emissionIntensity);
    }

    private void Start()
    {
        _nodeView.OnStart();
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
        if (_player != null && _nodeType == NodeType.Blue) _player.onDash += UseHability;

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
        else
        {
            if (_player != null && _nodeType == NodeType.Blue) _player.onDash -= UseHability;

            _nodeView.EnableColl(true);
        }

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

    public void UseHability(float dashDuration, float dashCD)
    {
        StartCoroutine(_nodeView.ResetHability(dashDuration, dashCD));
    }
}
