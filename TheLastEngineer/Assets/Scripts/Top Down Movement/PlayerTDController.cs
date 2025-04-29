using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using MaskTransitions;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerTDController : MonoBehaviour, IMovablePassenger
{
    [SerializeField] private PlayerData _playerData;

    [Header("MVC Player View")]
    [SerializeField] private Outline _outline;
    [SerializeField] private ParticleSystem[] _ps;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _walkSource;
    [SerializeField] private AudioSource _fxSource;

    public static PlayerTDController Instance = null;
    [SerializeField] private NodeEffectController _nodeEffectController;

    private CharacterController _cc = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;
    private NodeController _node = default;

    private List<IInteractable> _interactables = default;
    
    private float _currentSpeed = default;
    private Vector3 _movement = default;
    private NodeType _currentNodeType = NodeType.None;
    private bool _dropAvailable = true;

    public bool DropAvailable { get => _dropAvailable; }

    #region -----STATES VARIABLES-----
    private IPlayerState _currentState = default;
    private PlayerEmptyState _playerEmptyState = default;
    private PlayerGrabState _playerGrabState = default;

    public PlayerEmptyState EmptyState { get { return _playerEmptyState; } }
    public PlayerGrabState GrabState { get { return _playerGrabState; } }
    #endregion

    public Action<float, float> OnDash;
    public Action<NodeController> OnChangeActiveShield;
    public Action<bool, NodeType> OnNodeGrabed;

    [HideInInspector] public Vector3 attachPos = new Vector3(0, 1f, 1.2f);

    #region -----CHECKERS FOR PLAYER ACTIONS-----
    public float GetHoldInteractionTime() => _playerData.holdInteractionTime;
    public bool HasNode() => _node != null;
    private bool CheckShieldAvialable() => _currentNodeType == NodeType.Green;
    private bool CheckDashAvialable() => _playerModel.CanDash;
    public NodeController GetCurrentNode() => _node;
    public Color CurrentNodeOutlineColor() => _node != null ? _node.OutlineColor : Color.black;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _playerEmptyState = new PlayerEmptyState();
        _playerGrabState = new PlayerGrabState();
        _interactables = new List<IInteractable>();

        Materializer.OnPlayerInsideTrigger += CheckDropAvailable;
    }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        
        _currentSpeed = _playerData.moveSpeed;

        _playerModel = new PlayerTDModel(_cc, transform, _playerData);
        _playerView = new PlayerTDView(_outline, _ps, _animator, _walkSource, _fxSource, _playerData);

        _playerModel.onDashCDFinished += _playerView.DashChargedSound;
        
        SetState(_playerEmptyState);
        StartInputs();
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

    //-----
    //  OnPlatformMoving MAKES THE PLAYER MOVE IN THE DIRECTION OF THE PLATFORM THAT HAS AS A TRANSFORM PARENT
    //  -----//
    public void OnPlatformMoving(Vector3 displacement)
    {
        _playerModel.OnPlatformMoving(displacement);
    }

    //-----
    //  WalkSound IS ONLY USED IN ANIMATIONS TO REPRODUCE WALK SOUND EFFECT
    //  -----//
    private void WalkSound()
    {
        if (!_playerModel.IsDashing && _cc.isGrounded)
            _playerView.WalkSound();
    }

    #region -----INPUTS MANAGEMENT-----
    private void OnDestroy()
    {
        InputManager.Instance.onInputsEnabled -= OnEnableInputs;
        InputManager.Instance.onInputsDisabled -= OnDisableInputs;
    }

    private void StartInputs()
    {
        InputManager.Instance.onInputsEnabled += OnEnableInputs;
        InputManager.Instance.onInputsDisabled += OnDisableInputs;

        if (InputManager.Instance.playerInputs.Player.enabled) OnEnableInputs();
    }

    public void OnEnableInputs()
    {
        InputManager.Instance.dashInput.performed += GetDashKey;
        InputManager.Instance.interactInput.started += GetInteractionKey;
        InputManager.Instance.interactInput.canceled += CanceledHoldIInteraction;
        InputManager.Instance.shieldInput.performed += GetShieldKey;
    }

    public void OnDisableInputs()
    {
        InputManager.Instance.dashInput.performed -= GetDashKey;
        InputManager.Instance.interactInput.started -= GetInteractionKey;
        InputManager.Instance.interactInput.canceled -= CanceledHoldIInteraction;
        InputManager.Instance.shieldInput.performed -= GetShieldKey;
    }

    private void GetShieldKey(InputAction.CallbackContext context)
    {
        if (CheckShieldAvialable())
            OnChangeActiveShield?.Invoke(_node);
        else
            _playerView.PlayErrorSound(_playerData.emptyHand);
    }

    private void GetDashKey(InputAction.CallbackContext context)
    {
        if (_playerModel.CanDash)
        {
            InputManager.Instance.RumblePulse(_playerData.lowRumbleFrequency, _playerData.highRumbleFrequency, _playerData.rumbleDuration);
            StartCoroutine(_playerModel.Dash(GetMovement(), _currentNodeType));
            _playerView.DashSound();
            OnDash?.Invoke(_playerData.dashDuration, _playerData.dashCD);
            StartCoroutine(_playerModel.DashCD());
        }
        else
            _playerView.PlayErrorSound(_playerData.emptyHand);
    }

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
            .Where(i => i.CanInteract(this) && IsInFOV(i.Transform))
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => Vector3.Distance(transform.position, i.Transform.position))
            .FirstOrDefault();
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        if (_interactables.Contains(interactable))
            _interactables.Remove(interactable);
    }

    public void SetState(IPlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(this);
    }

    #region -----NODE MANAGEMENT-----
    public void DropOrGrabNode(bool grabbed)
    {
        OnNodeGrabed?.Invoke(grabbed, _node.NodeType);
    }

    public void PickUpNode(NodeController node)
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

        _node.Attach(_node.transform.position);
        ResetNode();
    }

    private void ResetNode()
    {
        _node = null;
        _currentNodeType = NodeType.None;
        _playerView.GrabNode();
    }
    #endregion

    public bool CheckForWalls()
    {
        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);

        if (Physics.Raycast(rayPos, transform.forward, 3f, _playerData.wallMask))
            return true;

        return false;
    }

    public bool CheckForWalls(NodeController node)
    {
        if (Physics.Raycast(transform.position, node.transform.position, 7f, _playerData.wallMask))
            return true;
        
        return false;
    }

    public bool IsInFOV(Transform interactable)
    {
        Vector3 playerPos = new Vector3(transform.position.x - 1f, transform.position.y + 2f, transform.position.z) - (transform.forward / 0.8f); 

        Vector3 dir = (interactable.position - playerPos).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        
        return angle <= _playerData.fovAngle * 0.5f;
    }

    public void CheckDropAvailable(bool available)
    {
        _dropAvailable = !available;
    }

    public void ResetLevel()
    {
        TransitionManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
    }

    #region -----TRIGGERS MANAGEMENT-----
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
            _interactables.Add(interactable);

        else if (coll.CompareTag("Void")) ResetLevel();
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
            _interactables.Remove(interactable);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z) - (transform.forward / 0.8f);
        Gizmos.DrawRay(rayPos, transform.forward * 3f);

        // Dibujar el cono de FOV
        Gizmos.color = Color.yellow;
        float halfFOV = _playerData.fovAngle * 0.5f;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfFOV, 0) * transform.forward * 5f;
        Vector3 rightBoundary = Quaternion.Euler(0, halfFOV, 0) * transform.forward * 5f;
        Gizmos.DrawRay(rayPos, leftBoundary);
        Gizmos.DrawRay(rayPos, rightBoundary);
    }
}
