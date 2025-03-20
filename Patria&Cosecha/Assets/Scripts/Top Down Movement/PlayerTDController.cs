using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private AudioClip _chargedDashClip;
    [SerializeField] private AudioClip _liftClip;
    [SerializeField] private AudioClip _putDownClip;
    [SerializeField] private AudioClip _emptyHand;

    private PlayerInputs _playerInputs = default;
    private InputAction _moveInput = default, _interactInput = default, _dashInput = default;

    private CharacterController _cc = default;

    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;
    private CombineMachine _combineMachine = default;
    private CombinerController _combiner = default;

    private float _commonSpeed = default, _verticalInput = default, _horizontalInput = default;
    private bool _dashKeyPressed = false, _interactKeyPressed = false;
    private Vector3 _movement = default;

    private PlayerState _playerState = PlayerState.Empty;
    private NodeType _currentNodeType = NodeType.None;

    private bool CanDash { get { return CheckDashAvialable(); } }
    private bool IsInConnectArea { get { return _connectionNode != null; } }
    private bool IsInCombinationArea { get { return _combineMachine != null; } }
    private bool IsInCombinerArea { get { return _combiner != null; } }

    public delegate void OnDash(float dashDuration, float dashCD);
    public event OnDash onDash = default;

    private void Awake()
    {
        _playerInputs = new PlayerInputs();
    }

    private void OnEnable()
    {
        _moveInput = _playerInputs.Player.Move;
        _interactInput = _playerInputs.Player.Interact;
        _dashInput = _playerInputs.Player.Dash;

        _moveInput.Enable();
        _interactInput.Enable();
        _dashInput.Enable();

        _dashInput.performed += GetDashKey;
        _interactInput.performed += GetInteractKey;
    }

    private void OnDisable()
    {
        _dashInput.performed -= GetDashKey;
        _interactInput.performed -= GetInteractKey;

        _moveInput.Disable();
        _interactInput.Disable();
        _dashInput.Disable();
    }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();

        _commonSpeed = _moveSpeed;

        _playerModel = new PlayerTDModel(_cc, transform, _moveSpeed, _rotSpeed, _dashSpeed, _dashDuration, _dashCD);
        _playerView = new PlayerTDView(_outline, _ps, _animator, _walkSource, _fxSource, _walkClip, _dashClip, _chargedDashClip, _liftClip, _putDownClip);

        _playerModel.onDashCDFinished += _playerView.DashChargedSound;
    }

    private void Update()
    {
        _playerModel.OnUpdate(GetMovement(), _moveSpeed);
        _playerView.Walk(GetMovement());

        if (_dashKeyPressed)
        {
            if (CanDash && _playerModel.CanDash)
            {
                StartCoroutine(_playerModel.Dash(GetMovement(), _currentNodeType));
                _playerView.DashSound();
                onDash?.Invoke(_dashDuration, _dashCD);
                StartCoroutine(_playerModel.DashCD());
            }
            else
                _playerView.PlayErrorSound(_emptyHand);

            _dashKeyPressed = false;
        }

        if (_interactKeyPressed && _cc.isGrounded && !_playerModel.IsDashing)
        {
            CheckInteraction();
            _interactKeyPressed = false;
        }
    }

    private Vector3 GetMovement()
    {
        _movement = _moveInput.ReadValue<Vector2>();

        return new Vector3(_movement.x, 0, _movement.y);
    }

    private void WalkSound()
    {
        if (!_playerModel.IsDashing && _cc.isGrounded)
            _playerView.WalkSound();
    }

    private void GetDashKey(InputAction.CallbackContext context)
    {
        _dashKeyPressed = true;
    }
    private void GetInteractKey(InputAction.CallbackContext context)
    {
        _interactKeyPressed = true;
    }

    private bool CheckDashAvialable() => _currentNodeType == NodeType.Blue || _currentNodeType == NodeType.Dash;

    private void CheckInteraction()
    {
        if (_node != null && _currentNodeType == NodeType.None && _playerState == PlayerState.Empty && !CheckForWalls()) ChangeNode();
        else if (_node != null && _currentNodeType != NodeType.None && _playerState == PlayerState.Grab)
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
        _currentNodeType = _node.NodeType;

        Vector3 attachPos = new Vector3(0, 1f, 1.2f);
        _node.Attach(this, attachPos);

        _playerView.GrabNode(true, _node.OutlineColor);
        _playerState = PlayerState.Grab;
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
        _playerState = PlayerState.Empty;
    }

    private void CheckCurrentNode()
    {
        if (_currentNodeType == NodeType.Purple || _currentNodeType == NodeType.Dash)
            _moveSpeed += 5;
        else
            _moveSpeed = _commonSpeed;
    }

    private void ResetNode()
    {
        _node = null;
        _currentNodeType = NodeType.None;
        _playerState = PlayerState.Empty;
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

        if (node != null && _currentNodeType == NodeType.None) _node = node;
        else if (connectionNode != null) _connectionNode = connectionNode;
        else if (machine != null) _combineMachine = machine;
        else if (combiner != null) _combiner = combiner;

        else if (coll.CompareTag("Void")) ResetLevel();
    }

    // THIS METHOD IS USED TO GRAB A NODE WHEN ANOTHER ONE WAS DROP NEARBY
    private void OnTriggerStay(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();

        if (node != null && _currentNodeType == NodeType.None) _node = node;
    }

    private void OnTriggerExit(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();
        CombineMachine machine = coll.GetComponent<CombineMachine>();
        CombinerController combiner = coll.GetComponent<CombinerController>();

        if (node != null && _currentNodeType == NodeType.None) _node = null;
        else if (connectionNode != null) _connectionNode = null;
        else if (machine != null) _combineMachine = null;
        else if (combiner != null) _combiner = null;
    }
}
