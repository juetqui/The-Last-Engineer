using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private NodeType _nodeType;
    [SerializeField] private Collider _collider;
    [SerializeField] private float _rotSpeed, _minY, _maxY, _moveSpeed;
    [SerializeField] private bool _isChildren = false;

    private NodeView _nodeView = default;
    private ConnectionNode _connectionNode = default;
    private bool _isConnected = false;

    public NodeType NodeType { get { return _nodeType; } }
    public bool IsChildren { get { return _isChildren; } }
    public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }

    private void Start()
    {
        _nodeView = new NodeView(_renderer, _material);
        _nodeView.OnStart();
    }

    private void Update()
    {
        if (!_isChildren) MoveObject();
        if (_isConnected) _collider.enabled = false;
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
        if (_isConnected) return;
        
        _collider.enabled = false;
        Attach(player.transform, newPos, true);

        transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        if (_isChildren && _connectionNode != null)
        {
            _connectionNode.UnsetNode();
            _connectionNode = null;
        }
    }

    public void Attach(Transform newParent, Vector3 newPos, bool parentIsPlayer = false)
    {
        if (!parentIsPlayer)
        {
            _collider.enabled = true;
            _connectionNode = newParent.GetComponent<ConnectionNode>();
            transform.localScale = Vector3.one;
        }
        
        _isChildren = true;
        
        transform.SetParent(newParent, false);
        transform.localPosition = newPos;
        transform.rotation = Quaternion.LookRotation(newParent.forward);
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
