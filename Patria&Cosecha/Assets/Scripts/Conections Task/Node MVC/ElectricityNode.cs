using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private Collider _collider;
    
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

    private void Start()
    {
        _nodeModel = new NodeModel(transform);
        _nodeView = new NodeView(_renderer, _material, _collider, _outline, _outlineColor);
        _nodeView.OnStart();
    }

    private void Update()
    {
        if (!_isChildren) MoveObject();
        //if (_isConnected) _nodeView.EnableColl(false);
    }

    private void MoveObject()
    {
        _collider.enabled = true;

        Vector3 currentPosition = transform.position;
        float newY = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);

        transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        transform.Rotate(0, _rotSpeed * Time.deltaTime, 0);
    }

    public void Attach(PlayerTDController player, Vector3 newPos)
    {
        Vector3 newScale = new Vector3(0.6f, 0.6f, 0.6f);
        
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

        if (_nodeType == NodeType.Dash) Debug.Log(_combineMachine);

        if (newParent != null) _isChildren = true;
        else _isChildren = false;

        if (newParent != null && newScale != default) _nodeModel.SetPos(newPos, newParent, newScale);
        else if (newParent != null && newScale == default) _nodeModel.SetPos(newPos, newParent);
        else if (newParent == null && newScale == default) _nodeModel.SetPos(newPos);
    }

    public void Attach(Vector3 newPos, CombineMachine newParent, Vector3 newScale)
    {
        _nodeView.EnableColl(true);

        _combineMachine = newParent;

        _isChildren = true;

        if (newParent != null && newScale != default) _nodeModel.SetPos(newPos, newParent.transform, newScale);
    }

    public void Combine(float deltaTime)
    {
        _nodeModel.Combine(deltaTime);
        _nodeView.EnableColl(false);
    }
}

public enum NodeType
{
    None,
    Purple,
    Green,
    Blue,
    Dash
}
