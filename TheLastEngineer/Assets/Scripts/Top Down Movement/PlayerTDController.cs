using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using MaskTransitions;
using System.Collections.Generic;
using System.Linq;

public class PlayerTDController : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;

    [Header("MVC Player View")]
    [SerializeField] private Outline _outline;
    [SerializeField] private ParticleSystem[] _ps;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _walkSource;
    [SerializeField] private AudioSource _fxSource;

    private CharacterController _cc = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;
    private ElectricityNode _node = default;

    private List<IInteractable> _interactables = default;
    
    private float _currentSpeed = default;
    private Vector3 _movement = default;
    private NodeType _currentNodeType = NodeType.None;


    #region -----STATES VARIABLES-----
    private IPlayerState _currentState = default;
    private PlayerEmptyState _playerEmptyState = default;
    private PlayerGrabState _playerGrabState = default;

    public PlayerEmptyState EmptyState { get { return _playerEmptyState; } }
    public PlayerGrabState GrabState { get { return _playerGrabState; } }
    #endregion

    private bool CanDash { get { return CheckDashAvialable(); } }

    public delegate void OnDash(float dashDuration, float dashCD);
    public event OnDash onDash = default;

    [HideInInspector] public Vector3 attachPos = new Vector3(0, 1f, 1.2f);

    #region -----CHECKERS FOR PLAYER ACTIONS-----
    public float GetHoldInteractionTime() => _playerData.holdInteractionTime;
    public bool HasNode() => _node != null;
    private bool CheckDashAvialable() => _currentNodeType == NodeType.Blue || _currentNodeType == NodeType.Dash;
    private bool UpgradedSpeedAvailable() => _currentNodeType == NodeType.Purple || _currentNodeType == NodeType.Dash;
    public ElectricityNode GetCurrentNode() => _node;
    public Color CurrentNodeOutlineColor() => _node != null ? _node.OutlineColor : Color.black;
    #endregion

    private void Awake()
    {
        _playerEmptyState = new PlayerEmptyState();
        _playerGrabState = new PlayerGrabState();
        _interactables = new List<IInteractable>();
        
        InputManager.Instance.onInputsEnabled += OnEnableInputs;
        InputManager.Instance.onInputsDisabled += OnDisableInputs;
    }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        
        _currentSpeed = _playerData.moveSpeed;

        _playerModel = new PlayerTDModel(_cc, transform, _playerData);
        _playerView = new PlayerTDView(_outline, _ps, _animator, _walkSource, _fxSource, _playerData);

        _playerModel.onDashCDFinished += _playerView.DashChargedSound;
        
        SetState(_playerEmptyState);
    }

    private void Update()
    {
        _playerModel.OnUpdate(GetMovement(), _currentSpeed);
        _playerView.Walk(GetMovement());
    }

    private Vector3 GetMovement()
    {
        _movement = InputManager.Instance.moveInput.ReadValue<Vector2>();

        return new Vector3(_movement.x, 0, _movement.y);
    }

    //----- WalkSound() IS ONLY USED IN ANIMATIONS TO REPRODUCE WALK SOUND EFFECT -----//
    private void WalkSound()
    {
        if (!_playerModel.IsDashing && _cc.isGrounded)
            _playerView.WalkSound();
    }

    #region -----INPUTS MANAGEMENT-----
    public void OnEnableInputs()
    {
        InputManager.Instance.dashInput.performed += GetDashKey;
        //InputManager.Instance.interactInput.performed += GetInteractKey;

        InputManager.Instance.interactInput.started += GetInteractionKey;
        InputManager.Instance.interactInput.canceled += CanceledHoldIInteraction;
    }

    public void OnDisableInputs()
    {
        InputManager.Instance.dashInput.performed -= GetDashKey;
        //InputManager.Instance.interactInput.performed -= GetInteractKey;

        InputManager.Instance.interactInput.started -= GetInteractionKey;
        InputManager.Instance.interactInput.canceled -= CanceledHoldIInteraction;
    }

    private void GetDashKey(InputAction.CallbackContext context)
    {
        if (CanDash && _playerModel.CanDash)
        {
            InputManager.Instance.RumblePulse(_playerData.lowRumbleFrequency, _playerData.highRumbleFrequency, _playerData.rumbleDuration);
            StartCoroutine(_playerModel.Dash(GetMovement(), _currentNodeType));
            _playerView.DashSound();
            onDash?.Invoke(_playerData.dashDuration, _playerData.dashCD);
            StartCoroutine(_playerModel.DashCD());
        }
        else
            _playerView.PlayErrorSound(_playerData.emptyHand);
    }

    //private void GetInteractKey(InputAction.CallbackContext context)
    //{
    //    if (_cc.isGrounded && !_playerModel.IsDashing)
    //    {
    //        var interactableTarget = GetClosestInteractable();
    //        _currentState?.HandleInteraction(interactableTarget);
    //    }
    //}

    private void GetInteractionKey(InputAction.CallbackContext context)
    {
        if (_cc.isGrounded && !_playerModel.IsDashing)
        {
            var interactableTarget = GetClosestInteractable();
            _currentState?.HandleInteraction(interactableTarget);
        }
    }

    private void CanceledHoldIInteraction(InputAction.CallbackContext context)
    {
        _playerEmptyState.CancelInteraction();
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
        if (UpgradedSpeedAvailable())
        {
            _currentSpeed = _playerData.upgradedMoveSpeed;
        }
        else
        {
            _currentSpeed = _playerData.moveSpeed;
        }
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
        if (Physics.Raycast(transform.position, _node.transform.position, 7f, _playerData.wallMask))
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
