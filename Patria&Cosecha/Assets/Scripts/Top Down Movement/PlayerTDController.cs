using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotSpeed;
    [SerializeField] private LayerMask _groundMask;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDrag;
    [SerializeField] private float _dashCooldown;

    [Header("View")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _grabClip;
    
    private Rigidbody _rb = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;
    private CombineMachine _combineMachine = default;

    private NodeType _currentType = NodeType.None;

    private bool CanDash { get { return _currentType == NodeType.Dash; } }
    private bool IsInConnectArea { get { return _connectionNode != null; } }
    private bool IsInCombineArea { get { return _combineMachine != null; } }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _playerModel = new PlayerTDModel(_rb, transform, _groundMask, _moveSpeed, _rotSpeed, _dashSpeed, _dashDrag, _dashCooldown);
        _playerView = new PlayerTDView(_animator, _source, _walkClip, _grabClip);
    }

    private void Update()
    {
        _playerModel.DashSpeed = _dashSpeed;
        _playerModel.DashDrag = _dashDrag;

        _playerView.Walk(GetMovement());

        _playerModel.UpdateDashTimer(Time.deltaTime);

        if (CheckForDash()) _playerModel.Dash(GetMovement());

        if (Input.GetKeyDown(KeyCode.L)) ResetLevel();
        if (Input.GetKeyDown(KeyCode.E)) CheckInteraction();
    }

    private void FixedUpdate()
    {
        if (!_playerModel.IsDashing) _playerModel.OnUpdate(GetMovement(), Time.deltaTime);
    }

    private Vector3 GetMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        return new Vector3(horizontalInput, 0, verticalInput);
    }

    private bool CheckForDash() => CanDash && !_playerModel.IsDashing && Input.GetKeyDown(KeyCode.Space);

    private void CheckInteraction()
    {
        if (_node != null && _currentType == NodeType.None) ChangeNode();
        else if (_node != null && _currentType != NodeType.None)
        {
            if (_connectionNode != null && IsInConnectArea) PlaceNode();
            if (_combineMachine != null && IsInCombineArea) PlaceInMachine();
        }
    }

    private void ChangeNode()
    {
        _currentType = _node.NodeType;

        Vector3 attachPos = new Vector3(0, 1f, 1.2f);
        _node.Attach(this, attachPos);
        _playerView.GrabNode();
    }

    private void PlaceNode()
    {
        if (_connectionNode.IsDisabled) return;

        _connectionNode.SetNode(_node);
        _connectionNode = null;
        ResetNode();
    }

    private void PlaceInMachine()
    {
        _combineMachine.SetNode(_node);
        ResetNode();
    }

    private void ResetNode()
    {
        _node = null;
        _currentType = NodeType.None;
    }

    private void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTriggerEnter(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();
        CombineMachine machine = coll.GetComponent<CombineMachine>();

        if (node != null && _currentType == NodeType.None) _node = node;
        else if (connectionNode != null) _connectionNode = connectionNode;
        else if (machine != null) _combineMachine = machine;
        else if (coll.CompareTag("Void")) ResetLevel();
    }

    private void OnTriggerExit(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();
        CombineMachine machine = coll.GetComponent<CombineMachine>();

        if (node != null && _currentType == NodeType.None) _node = null;
        else if (connectionNode != null) _connectionNode = null;
        else if (machine != null) _combineMachine = null;
    }
}
