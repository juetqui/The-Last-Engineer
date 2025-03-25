using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using MaskTransitions;
using System.Collections.Generic;
using System.Linq;

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

    private List<IInteractable> _interactables = default;
    
    private ElectricityNode _node = default;
    
    private float _commonSpeed = default, _verticalInput = default, _horizontalInput = default;
    private Vector3 _movement = default;

    private NodeType _currentNodeType = NodeType.None;
    
    private IPlayerState _currentState = default;
    private PlayerEmptyState _playerEmptyState = default;
    private PlayerGrabState _playerGrabState = default;

    public PlayerEmptyState EmptyState { get { return _playerEmptyState; } }
    public PlayerGrabState GrabState { get { return _playerGrabState; } }

    private bool CanDash { get { return CheckDashAvialable(); } }

    public delegate void OnDash(float dashDuration, float dashCD);
    public event OnDash onDash = default;

    [HideInInspector] public Vector3 attachPos = new Vector3(0, 1f, 1.2f);

    #region -----CHECKERS FOR PLAYER ACTIONS-----
    public bool HasNode() => _node != null;
    public ElectricityNode GetCurrentNode() => _node;
    public Color CurrentNodeOutlineColor() => _node != null ? _node.OutlineColor : Color.black;
    private bool CheckDashAvialable() => _currentNodeType == NodeType.Blue || _currentNodeType == NodeType.Dash;
    #endregion

    private void Awake()
    {
        _playerInputs = new PlayerInputs();
        _playerEmptyState = new PlayerEmptyState();
        _playerGrabState = new PlayerGrabState();
        _interactables = new List<IInteractable>();
    }

    #region -----INPUTS MANAGEMENT-----
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
    #endregion

    private void Start()
    {
        _cc = GetComponent<CharacterController>();

        _commonSpeed = _moveSpeed;

        _playerModel = new PlayerTDModel(_cc, transform, _moveSpeed, _rotSpeed, _dashSpeed, _dashDuration, _dashCD);
        _playerView = new PlayerTDView(_outline, _ps, _animator, _walkSource, _fxSource, _walkClip, _dashClip, _chargedDashClip, _liftClip, _putDownClip);

        _playerModel.onDashCDFinished += _playerView.DashChargedSound;
        
        SetState(_playerEmptyState);
    }

    private void Update()
    {
        _playerModel.OnUpdate(GetMovement(), _moveSpeed);
        _playerView.Walk(GetMovement());
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

    #region -----INPUTS-----
    private void GetDashKey(InputAction.CallbackContext context)
    {
        if (CanDash && _playerModel.CanDash)
        {
            StartCoroutine(_playerModel.Dash (GetMovement(), _currentNodeType));
            _playerView.DashSound();
            onDash?.Invoke(_dashDuration, _dashCD);
            StartCoroutine(_playerModel.DashCD());
        }
        else
            _playerView.PlayErrorSound(_emptyHand);
    }
    
    private void GetInteractKey(InputAction.CallbackContext context)
    {
        if (_cc.isGrounded && !_playerModel.IsDashing)
        {
            var interactableTarget = GetClosestInteractable();
            _currentState?.HandleInteraction(interactableTarget);
        }
    }
    #endregion

    private IInteractable GetClosestInteractable()
    {
        return _interactables
            .Where(i => i.CanInteract(this))
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => Vector3.Distance(transform.position, i.Transform.position))
            .FirstOrDefault();
    }

    public void SetState(IPlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(this);
    }

    #region -----NODE MANAGEMENT-----
    public void PickUpNode(ElectricityNode node)
    {
        if (_node != null || node == null) return;

        _node = node;
        _currentNodeType = _node.NodeType;
        _playerView.GrabNode(true, _node.OutlineColor);
        RemoveInteractable(node);
    }

    public void ReleaseNode(IInteractable interactable)
    {
        ResetNode();
        RemoveInteractable(interactable);
    }

    public void DropNode()
    {
        if (_node == null) return;

        _node.Attach(transform.position);
        ResetNode();
    }

    public void CheckCurrentNode()
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
        _playerView.GrabNode();
    }
    #endregion

    public void RemoveInteractable(IInteractable interactable)
    {
        if (_interactables.Contains(interactable))
        {
            _interactables.Remove(interactable);
        }
    }

    public bool CheckForWalls()
    {
        if (Physics.Raycast(transform.position, _node.transform.position, 7f, _wallMask))
            return true;
        
        return false;
    }

    public void ResetLevel()
    {
        TransitionManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
    }

    #region -----TRIGGERS MANAGEMENT-----
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent<IInteractable>(out var interactable))
        {
            _interactables.Add(interactable);
        }

        else if (coll.CompareTag("Void")) ResetLevel();
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent<IInteractable>(out var interactable))
        {
            _interactables.Remove(interactable);
        }
    }
    #endregion
}
