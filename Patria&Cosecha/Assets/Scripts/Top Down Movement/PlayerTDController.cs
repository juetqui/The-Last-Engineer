using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotSpeed;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private LayerMask _wallMask;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDrag;
    [SerializeField] private float _dashCooldown;

    [Header("View")]
    [SerializeField] private Outline _outline;
    [SerializeField] private ParticleSystem[] _ps;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _source;

    [Header("Audio")]
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _liftClip;
    [SerializeField] private AudioClip _putDownClip;
    [SerializeField] private AudioClip _emptyHand;

    private Rigidbody _rb = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;
    private CombineMachine _combineMachine = default;
    private CombinerController _combiner = default;

    private float _commonSpeed = default;

    private NodeType _currentType = NodeType.None;

    private bool CanDash { get { return CheckDashAvialable(); } }
    private bool IsInConnectArea { get { return _connectionNode != null; } }
    private bool IsInCombinationArea { get { return _combineMachine != null; } }
    private bool IsInCombinerArea { get { return _combiner != null; } }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _commonSpeed = _moveSpeed;

        _playerModel = new PlayerTDModel(_rb, transform, _groundMask, _moveSpeed, _rotSpeed, _dashSpeed, _dashDrag, _dashCooldown);
        _playerView = new PlayerTDView(_outline, _ps, _animator, _source, _walkClip, _liftClip, _putDownClip);

        _playerModel.OnStart();
    }

    private void Update()
    {
        _playerModel.MoveSpeed = _moveSpeed;
        _playerModel.DashSpeed = _dashSpeed;
        _playerModel.DashDrag = _dashDrag;

        _playerView.Walk(GetMovement());

        _playerModel.UpdateDashTimer(Time.deltaTime);
        
        if (CheckForDash())
        {
            if (CanDash && !_playerModel.IsDashing)
                _playerModel.Dash(_currentType, GetMovement());
            else
                _playerView.PlayErrorSound(_emptyHand);
        }

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

    private bool CheckForDash() => Input.GetKeyDown(KeyCode.Space);

    private bool CheckDashAvialable() => _currentType == NodeType.Blue || _currentType == NodeType.Dash;

    private void CheckInteraction()
    {
        if (_node != null && _currentType == NodeType.None && !CheckForWalls()) ChangeNode();
        else if (_node != null && _currentType != NodeType.None)
        {
            if (_connectionNode != null && IsInConnectArea) PlaceNode();
            else if (_combineMachine != null && IsInCombinationArea) PlaceInMachine();
            else if (!IsInConnectArea && !IsInCombinationArea && !IsInCombinerArea) DropNode();

            _playerView.GrabNode();
        }

        if (_combiner != null && IsInCombinerArea)
        {
            _combiner.ActivateCombineMachine();
            _combiner = null;
        }

        CheckCurrentNode();
    }

    private void ChangeNode()
    {
        _currentType = _node.NodeType;

        Vector3 attachPos = new Vector3(0, 1f, 1.2f);
        _node.Attach(this, attachPos);

        _playerView.GrabNode(true, _node.OutlineColor);
    }

    private void DropNode()
    {
        Vector3 dropPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _node.Attach(dropPos);
        ResetNode();

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
        _combineMachine = null;
        ResetNode();
    }

    private void CheckCurrentNode()
    {
        if (_currentType == NodeType.Purple || _currentType == NodeType.Dash)
            _moveSpeed += 5;
        else
            _moveSpeed = _commonSpeed;
    }

    private void ResetNode()
    {
        _node = null;
        _currentType = NodeType.None;
    }

    private bool CheckForWalls()
    {
        if (Physics.Raycast(transform.position, _node.transform.position, 7f, _wallMask))
            return true;
        
        return false;
    }

    private void OnTriggerEnter(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();
        CombineMachine machine = coll.GetComponent<CombineMachine>();
        CombinerController combiner = coll.GetComponent<CombinerController>();

        if (node != null && _currentType == NodeType.None) _node = node;
        else if (connectionNode != null) _connectionNode = connectionNode;
        else if (machine != null) _combineMachine = machine;
        else if (combiner != null) _combiner = combiner;

        else if (coll.CompareTag("Void")) ResetLevel();
    }

    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // THIS METHOD IS USED TO GRAB A NODE WHEN ANOTHER ONE WAS DROP NEARBY
    private void OnTriggerStay(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();

        if (node != null && _currentType == NodeType.None) _node = node;
    }

    private void OnTriggerExit(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();
        CombineMachine machine = coll.GetComponent<CombineMachine>();
        CombinerController combiner = coll.GetComponent<CombinerController>();

        if (node != null && _currentType == NodeType.None) _node = null;
        else if (connectionNode != null) _connectionNode = null;
        else if (machine != null) _combineMachine = null;
        else if (combiner != null) _combiner = null;
    }

    private void OnDrawGizmos()
    {
        Vector3 rayDir = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
        Gizmos.DrawRay(rayDir, Vector3.down * 2);
    }
}
