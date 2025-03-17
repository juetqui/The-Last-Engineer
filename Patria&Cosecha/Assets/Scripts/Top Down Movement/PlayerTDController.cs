using UnityEngine;
using UnityEngine.SceneManagement;
using MaskTransitions;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotSpeed;
    [SerializeField] private LayerMask _wallMask;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashCD;

    [Header("View")]
    [SerializeField] private Outline _outline;
    [SerializeField] private ParticleSystem[] _ps;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _walkSource;
    [SerializeField] private AudioSource _fxSource;

    [Header("Audio")]
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _dashClip;
    [SerializeField] private AudioClip _liftClip;
    [SerializeField] private AudioClip _putDownClip;
    [SerializeField] private AudioClip _emptyHand;

    private CharacterController _cc = default;

    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;
    private CombineMachine _combineMachine = default;
    private CombinerController _combiner = default;

    private float _commonSpeed = default, _verticalInput = default, _horizontalInput = default;
    private NodeType _currentType = NodeType.None;

    private bool CanDash { get { return CheckDashAvialable(); } }
    private bool IsInConnectArea { get { return _connectionNode != null; } }
    private bool IsInCombinationArea { get { return _combineMachine != null; } }
    private bool IsInCombinerArea { get { return _combiner != null; } }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();

        _commonSpeed = _moveSpeed;

        _playerModel = new PlayerTDModel(_cc, transform, _moveSpeed, _rotSpeed, _dashSpeed, _dashDuration, _dashCD);
        _playerView = new PlayerTDView(_outline, _ps, _animator, _walkSource, _fxSource, _walkClip, _dashClip, _liftClip, _putDownClip);
    }

    private void Update()
    {
        _playerModel.OnUpdate(GetMovement(), _moveSpeed);
        _playerView.Walk(GetMovement());

        if (GetDashKey())
        {
            if (CanDash && _playerModel.CanDash)
            {
                StartCoroutine(_playerModel.Dash(GetMovement(), _currentType));
                _playerView.DashSound();
                StartCoroutine(_playerModel.DashCD());
            }
            else
                _playerView.PlayErrorSound(_emptyHand);
        }

        if (Input.GetKeyDown(KeyCode.E) && _cc.isGrounded) CheckInteraction();
    }

    private Vector3 GetMovement()
    {
        _verticalInput = Input.GetAxisRaw("Vertical");
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        return new Vector3(_horizontalInput, 0, _verticalInput);
    }

    private void WalkSound()
    {
        if (!_playerModel.IsDashing && _cc.isGrounded)
            _playerView.WalkSound();
    }

    private bool GetDashKey() => Input.GetKeyDown(KeyCode.Space);

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

    public void ResetLevel()
    {
        TransitionManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
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
}
