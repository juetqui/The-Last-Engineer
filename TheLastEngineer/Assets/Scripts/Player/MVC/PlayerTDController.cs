using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.UI;
using Cinemachine;

public class PlayerTDController : MonoBehaviour, IMovablePassenger, ILaserReceptor
{
    [SerializeField] private PlayerData _playerData;
    public bool IsCorrupted;
    [Header("MVC Player View")]
    [SerializeField] private Renderer _renderer = default;
    [SerializeField] private Outline _outline;
    [SerializeField] private ParticleSystem _walkPS;
    [SerializeField] private ParticleSystem _orbitPS;
    [SerializeField] private ParticleSystem _defaultPS;
    [SerializeField] private ParticleSystem _corruptedPS;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _walkSource;
    [SerializeField] private AudioSource _fxSource;
    [SerializeField] private Image _dashImage;
    [SerializeField] private float _deadTimer;

    public static PlayerTDController Instance = null;
    [SerializeField] private SolvingController _solvingController;

    public Collider playerCollider;
    public CharacterController _cc = default;
    private CinemachineImpulseSource _impulseSource = default;

    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;
    private NodeController _node = default;
    private GlitchActive _glitchActive = default;

    private List<IInteractable> _interactables = default;
    private bool _isDead = false;
    private float _currentSpeed = default;
    private Vector3 _movement = default, _checkPointPos = default, _absorvedCorruptionPos = default;
    private NodeType _currentNodeType = NodeType.None;
    private bool _dropAvailable = true, _canMove = true;
    private Coroutine _corruptionAbsorved = null;

    private Glitcheable _currentPlatform = null;

    public bool DropAvailable { get => _dropAvailable; }

    #region -----STATES VARIABLES-----
    private IPlayerState _currentState = default;
    private IPlayerState _lastState = default;
    private PlayerEmptyState _playerEmptyState = default;
    private PlayerGrabState _playerGrabState = default;

    public IPlayerState LastState {  get { return _lastState; } }
    public PlayerEmptyState EmptyState { get { return _playerEmptyState; } }
    public PlayerGrabState GrabState { get { return _playerGrabState; } }
    #endregion

    public Action<float, float> OnDash;
    public Action<NodeController> OnChangeActiveShield;
    public Action<bool, NodeType> OnNodeGrabed;
    public Action<bool> OnAbsorbCorruption;
    public Action<IInteractable> OnInteractableSelected;

    [HideInInspector] public Vector3 attachPos = new Vector3(0, 1f, 1.2f);

    private Action<Vector3> _onPlatformPosChangedModel;
    private Action<Vector3> _onPlatformPosChangedCheck;

    #region -----CHECKERS FOR PLAYER ACTIONS-----
    public float GetHoldInteractionTime() => _playerData.holdInteractionTime;
    public bool HasNode() => _node != null;
    private bool CheckCorruptionAvailable() => _currentNodeType == NodeType.Corrupted;
    private bool CheckCorruptionChangeAvailable() => _currentNodeType != NodeType.None;
    private bool CheckDashAvialable() => _playerModel.CanDash;
    public NodeController GetCurrentNode() => _node;
    public NodeType GetCurrentNodeType() => _currentNodeType;
    public Color CurrentNodeOutlineColor() => _node != null ? _node.OutlineColor : Color.black;
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _impulseSource = GetComponent<CinemachineImpulseSource>();

        _playerEmptyState = new PlayerEmptyState();
        _playerGrabState = new PlayerGrabState();
        _interactables = new List<IInteractable>();

        _glitchActive = GetComponent<GlitchActive>();

        _checkPointPos = transform.position;
    }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        
        _currentSpeed = _playerData.moveSpeed;

        _playerModel = new PlayerTDModel(_cc, transform, _playerData, GetComponent<Collider>());
        _playerView = new PlayerTDView(_renderer, _outline, _walkPS, _orbitPS, _animator, _walkSource, _fxSource, _playerData, _solvingController, _dashImage, _defaultPS, _corruptedPS);

        _playerView.OnStart();
        _solvingController.OnDissolveCompleted += OnDissolveCompleted;
        GlitchActive.Instance.OnChangeObjectState += CheckInteraction;

        SetState(_playerEmptyState);
        StartInputs();
    }

    private void CheckInteraction(Glitcheable glitcheable, InteractionOutcome outcome)
    {
        if (outcome.Result == InteractResult.Invalid)
            _playerView.PlayErrorSound(_playerData.emptyHand);
    }

    private void Update()
    {
        _playerModel.OnUpdate(GetMovement(), _currentSpeed);
        _playerView.Walk(GetMovement());
        _currentState?.Tick();
    }

    private void CanceledHoldIInteraction(InputAction.CallbackContext context)
    {
        _currentState?.Cancel();
    }


    private Vector3 GetMovement()
    {
        if (_isDead || !_canMove) return Vector3.zero;
        
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
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnInputsEnabled -= OnEnableInputs;
        InputManager.Instance.OnInputsDisabled -= OnDisableInputs;
    }


    private void StartInputs()
    {
        InputManager.Instance.OnInputsEnabled += OnEnableInputs;
        InputManager.Instance.OnInputsDisabled += OnDisableInputs;

        if (InputManager.Instance.playerInputs.Player.enabled) OnEnableInputs();
    }

    public void OnEnableInputs()
    {
        InputManager.Instance.dashInput.performed += GetDashKey;
        InputManager.Instance.interactInput.started += GetInteractionKey;
        InputManager.Instance.interactInput.canceled += CanceledHoldIInteraction;
        InputManager.Instance.corruptionChangeInput.performed += GetCorruptionChangeKey;
        InputManager.Instance.cancelInput.performed += CancelInteraction;
    }

    public void OnDisableInputs()
    {
        InputManager.Instance.dashInput.performed -= GetDashKey;
        InputManager.Instance.interactInput.started -= GetInteractionKey;
        InputManager.Instance.interactInput.canceled -= CanceledHoldIInteraction;
        InputManager.Instance.corruptionChangeInput.performed -= GetCorruptionChangeKey;
        InputManager.Instance.cancelInput.performed -= CancelInteraction;
    }

    private void GetCorruptionChangeKey(InputAction.CallbackContext context)
    {
        if (CheckCorruptionChangeAvailable() && _glitchActive != null)
            _glitchActive.ChangeObjectState();
        else
            _playerView.PlayErrorSound(_playerData.emptyHand);
    }

    private void GetDashKey(InputAction.CallbackContext context)
    {
        if (_playerModel.CanDashWithCoyoteTime() && GetMovement() != Vector3.zero && !_isDead)
        {
            InputManager.Instance.RumblePulse(_playerData.lowRumbleFrequency, _playerData.highRumbleFrequency, _playerData.rumbleDuration);
            StartCoroutine(_playerModel.Dash(GetMovement(), _currentNodeType));
            _impulseSource.GenerateImpulseWithForce(_playerData.testForce);
            _playerView.DashSound();
            OnDash?.Invoke(_playerData.dashDuration, _playerData.dashCD);
            StartCoroutine(_playerModel.DashCD());
            StartCoroutine(_playerView.DashCD(_playerData.dashCD));
        }
    }

    private void GetInteractionKey(InputAction.CallbackContext context)
    {
        if (_cc.isGrounded && !_playerModel.IsDashing)
        {
            var interactableTarget = GetClosestInteractable();

            if (interactableTarget != null)
            {
                Vector3 targetRot = new Vector3(interactableTarget.Transform.position.x, transform.position.y, interactableTarget.Transform.position.z);

                transform.LookAt(targetRot);
            }

            _currentState?.HandleInteraction(interactableTarget);
            OnInteractableSelected?.Invoke(interactableTarget);
        }
    }
    
    private void CancelInteraction(InputAction.CallbackContext context)
    {
        OnInteractableSelected?.Invoke(null);
    }

    #endregion

    private IInteractable GetClosestInteractable()
    {
        IInteractable best = null;
        int bestPriority = int.MinValue;
        float bestDist = float.MaxValue;
        Vector3 p = transform.position;

        for (int i = 0; i < _interactables.Count; i++)
        {
            var it = _interactables[i];
            if (!it.CanInteract(this)) continue;

            int prio = ((int)it.Priority);
            float dist = (it.Transform.position - p).sqrMagnitude;

            if (prio > bestPriority || (prio == bestPriority && dist < bestDist))
            {
                bestPriority = prio;
                bestDist = dist;
                best = it;
            }
        }
        return best;
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        if (_interactables.Contains(interactable))
            _interactables.Remove(interactable);
    }

    public void SetState(IPlayerState newState)
    {
        _lastState = _currentState;
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter(this);
    }

    #region -----NODE MANAGEMENT-----
    public void DropOrGrabNode(bool grabbed)
    {
        OnNodeGrabed?.Invoke(grabbed, _node.NodeType);
        _node.OnUpdatedNodeType += UpdateOnNodeChange;
    }

    private void UpdateOnNodeChange(NodeType nodeType)
    {
        _currentNodeType = nodeType;

        if (_corruptionAbsorved != null && _currentNodeType != NodeType.Corrupted)
        {
            StopCoroutine(_corruptionAbsorved);
            _corruptionAbsorved = null;
            OnAbsorbCorruption.Invoke(false);
            _playerView.UpdatePlayerMaterials(false);
            IsCorrupted = false;
        }

        _playerView.GrabNode(true, _node.OutlineColor);
        _playerView.PlayNodePS(_node.NodeType);
        OnNodeGrabed?.Invoke(true, _node.NodeType);
    }

    public void PickUpNode(NodeController node)
    {
        if (_node != null || node == null) return;

        _node = node;
        _currentNodeType = _node.NodeType;
        _playerView.GrabNode(true, _node.OutlineColor);
        _playerView.PlayNodePS(_node.NodeType);
        RemoveInteractable(node);
    }

    public void ReleaseNode()
    {
        ResetNode();
    }

    public void DropNode()
    {
        if (_node == null) return;
        
        _node.OnUpdatedNodeType -= UpdateOnNodeChange;
        _node.Attach(_node.transform.position);
        RemoveInteractable(_node);
        ResetNode();
    }

    private void ResetNode()
    {
        _node = null;
        _currentNodeType = NodeType.None;
        _playerView.GrabNode(false, Color.black);
    }
    #endregion

    public bool CheckForWalls()
    {
        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);

        if (Physics.Raycast(rayPos, transform.forward, 3f, _playerData.wallMask))
            return true;

        return false;
    }

    public void SetCheckPointPos(Vector3 newPos)
    {
        _checkPointPos = newPos;
    }

    public void LaserRecived()
    {
        if (_isDead) return;
        
        _isDead = true;
        _playerView.SetAnimatorSpeed(0f);
        _playerView.DeathSound();
        _solvingController?.BurnShader();
        
        if (InputManager.Instance.playerInputs.Player.enabled) OnDisableInputs();
    }
    public void StartDesintegratePlayer()
    {
        _solvingController.StartDesintegrateShader();
        _node.StartDesintegrateShader();

    }
    public void StopDesintegratePlayer()
    {
        _solvingController.StopDesintegrateShader();
        _node.StopDesintegrateShader();

    }
    public void SetDesintegratePlayer(float alpha)
    {
        _solvingController.SetDesintegrateShader(alpha);
        _node.SetDesintegrateShader(alpha);
    }
    public void LaserNotRecived() { return; }

    private void OnDissolveCompleted()
    {
        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator StartCorruption()
    {
        OnAbsorbCorruption.Invoke(true);
        IsCorrupted = true;
        _absorvedCorruptionPos = transform.position;
        _playerView.UpdatePlayerMaterials(true);

        yield return new WaitForSeconds(5f);

        OnAbsorbCorruption.Invoke(false);
        _playerView.UpdatePlayerMaterials(false);
        _playerModel.SetPos(_absorvedCorruptionPos);
        IsCorrupted = false;
        _corruptionAbsorved = null;
    }

    public IEnumerator RespawnPlayer()
    {
        if (_currentPlatform != null) UnsetPlatform(_currentPlatform);

        GlitchDeathController.Instance.TriggerGlitch();
        yield return new WaitForSeconds(1f);

        _cc.enabled = false;
        transform.position = _checkPointPos;
        _cc.enabled = true;

        yield return new WaitForSeconds(0.5f);

        _isDead = false;
        _playerView.SetAnimatorSpeed(1f);
        _solvingController?.RespawnPlayer();
        playerCollider.enabled = true;
        OnEnableInputs();
    }

    public void SetPlatform(Glitcheable platform)
    {
        if (_currentPlatform == platform) return;

        if (_currentPlatform != null) UnsetPlatform(_currentPlatform);

        _playerModel.SetGravity(false);
        _currentPlatform = platform;

        _onPlatformPosChangedModel = _playerModel.SetPos;
        _onPlatformPosChangedCheck = CheckPlatformMovement;

        _currentPlatform.OnPosChanged += _onPlatformPosChangedModel;
        _currentPlatform.OnPosChanged += _onPlatformPosChangedCheck;
    }

    public void UnsetPlatform(Glitcheable platform)
    {
        if (_currentPlatform != platform) return;

        if (_onPlatformPosChangedModel != null)
            platform.OnPosChanged -= _onPlatformPosChangedModel;

        if (_onPlatformPosChangedCheck != null)
            platform.OnPosChanged -= _onPlatformPosChangedCheck;

        _playerModel.SetGravity(true);
        _canMove = true;

        _onPlatformPosChangedModel = null;
        _onPlatformPosChangedCheck = null;
        _currentPlatform = null;
    }
    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
    }

    private void CheckPlatformMovement(Vector3 movement)
    {
        if (_currentPlatform != null)
        {
            _canMove = !(_currentPlatform.IsStopped == false);
        }
    }

    public void RemoveFromInteractables(IInteractable interactable)
    {
        if (_interactables.Contains(interactable))
            _interactables.Remove(interactable);
    }

    #region -----TRIGGERS MANAGEMENT-----
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
            _interactables.Add(interactable);

        else if (coll.CompareTag("Void")) 
        {
            _isDead = true;
            _playerView.FallSound();
            StartCoroutine(RespawnPlayer());
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
            _interactables.Remove(interactable);
    }
    #endregion
}
