using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour, IMovablePassenger, ILaserReceptor
{
    public static PlayerController Instance = null;

    [Header("Data & MVC")]
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Outline _outline;
    [SerializeField] private ParticleSystem _walkPS, _orbitPS, _defaultPS, _corruptedPS;
    [SerializeField] private AudioSource _walkSource, _fxSource;
    [SerializeField] private SolvingController _solvingController;

    public CharacterController CC { get; private set; }
    private Collider _collider = default;
    private Animator _animator = default;

    public Action<float, float> OnDash;
    public Action<IInteractable> OnInteractableSelected;

    // --- Internals
    private PlayerModel _model;
    public PlayerView View { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    
    private InputHandler _input;
    private GlitchActive _glitchActive;
    private PlayerNodeHandler _nodeHandler;
    private CinemachineImpulseSource _impulse;
    private InteractableHandler _interactableHandler;

    private Vector2 _move = Vector2.zero;
    private float _currentSpeed;
    private bool _isDead = false, _canMove = true;

    private Vector3 _checkPointPos;

    private void Awake()
    {
        if (Instance != null)
        {
            Instance = null;
            Destroy(Instance.gameObject);
        }

        Instance = this;
        
        CC = GetComponent<CharacterController>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        
        _input = GetComponent<InputHandler>();
        _glitchActive = GetComponent<GlitchActive>();
        _nodeHandler = GetComponent<PlayerNodeHandler>();
        _impulse = GetComponent<CinemachineImpulseSource>();
        _interactableHandler = new InteractableHandler();

        _checkPointPos = transform.position;
    }

    private void Start()
    {
        _currentSpeed = _playerData.moveSpeed;

        _model = new PlayerModel(CC, transform, _playerData, _collider);
        View = new PlayerView(_renderer, _outline, _walkPS, _orbitPS, _animator, _walkSource, _fxSource, _playerData, _solvingController, _defaultPS, _corruptedPS);
        View.OnStart();

        StateMachine = new PlayerStateMachine(this, _nodeHandler);
        _solvingController.OnDissolveCompleted += OnDissolveCompleted;
        
        GlitchActive.Instance.OnChangeObjectState += CheckInteractionOutcome;

        HookInputs(true);
    }

    private void OnDestroy()
    {
        HookInputs(false);

        if (GlitchActive.Instance != null)
            GlitchActive.Instance.OnChangeObjectState -= CheckInteractionOutcome;
        
        if (_solvingController != null)
            _solvingController.OnDissolveCompleted -= OnDissolveCompleted;
    }

    private void Update()
    {
        var mv3 = GetMovement3D();
        _model.OnUpdate(mv3, _currentSpeed);
        View.Walk(mv3);
        StateMachine.Tick();
    }

    private void HookInputs(bool enable)
    {
        if (_input == null) return;

        if (enable)
        {
            _input.OnMove += OnMove;
            _input.OnDash += OnDashPressed;
            _input.OnInteractStart += OnInteractPressed;
            _input.OnInteractCancel += OnInteractCanceled;
            _input.OnCorruptionChange += OnCorruptionChange;
            _input.OnCancelSelect += OnCancelSelect;
        }
        else
        {
            _input.OnMove -= OnMove;
            _input.OnDash -= OnDashPressed;
            _input.OnInteractStart -= OnInteractPressed;
            _input.OnInteractCancel -= OnInteractCanceled;
            _input.OnCorruptionChange -= OnCorruptionChange;
            _input.OnCancelSelect -= OnCancelSelect;
        }
    }

    #region INPUTS MANAGEMENT
    private void OnMove(Vector2 mv) => _move = _isDead || !_canMove ? Vector2.zero : mv;

    private void OnDashPressed()
    {
        if (_model.CanDashWithCoyoteTime() && _move != Vector2.zero && !_isDead)
        {
            InputManager.Instance.RumblePulse(_playerData.lowRumbleFrequency, _playerData.highRumbleFrequency, _playerData.rumbleDuration);
            StartCoroutine(_model.Dash(GetMovement3D(), _nodeHandler.CurrentType));
            _impulse.GenerateImpulseWithForce(_playerData.testForce);
            View.DashSound();
            OnDash?.Invoke(_playerData.dashDuration, _playerData.dashCD);
            StartCoroutine(_model.DashCD());
        }
    }

    private void OnInteractPressed()
    {
        if (!CC.isGrounded || _model.IsDashing) return;

        var target = _interactableHandler.GetInteractable(_nodeHandler, transform.position);
        
        if (target != null)
        {
            var look = new Vector3(target.Transform.position.x, transform.position.y, target.Transform.position.z);
            transform.LookAt(look);
        }

        StateMachine.CurrentState?.HandleInteraction(target);
        OnInteractableSelected?.Invoke(target);
    }

    private void OnInteractCanceled()
    {
        StateMachine.CurrentState?.Cancel();
    }

    private void OnCorruptionChange()
    {
        if (_glitchActive != null && _nodeHandler.CurrentType != NodeType.None)
            _glitchActive.ChangeObjectState();
        else
            View.PlayErrorSound(_playerData.emptyHand);
    }

    private void OnCancelSelect() => OnInteractableSelected?.Invoke(null);

    private Vector3 GetMovement3D()
    {
        if (_isDead || !_canMove) return Vector3.zero;
        if (_model.IsDashing) _move *= 0.01f;
        
        return new Vector3(_move.x, 0f, _move.y);
    }

    private void CheckInteractionOutcome(Glitcheable g, InteractionOutcome outcome)
    {
        if (outcome.Result == InteractResult.Invalid)
            View.PlayErrorSound(_playerData.emptyHand);
    }

    public bool CheckForWalls()
    {
        Vector3 rayPos = new(transform.position.x, transform.position.y + 2f, transform.position.z);
        return Physics.Raycast(rayPos, transform.forward, 3f, _playerData.wallMask);
    }
    #endregion

    #region STATE MACHINE API
    public void PickUpNode(NodeController node) => _nodeHandler.Pick(node);
    public void ReleaseNode() => _nodeHandler.Release();
    public void DropNode() => _nodeHandler.Release(true);

    public float GetHoldInteractionTime() => _playerData.holdInteractionTime;

    public void RemoveInteractable(IInteractable interactable) => _interactableHandler.Remove(interactable);
    #endregion

    #region PLATFORM TP MANAGEMENT
    public void OnPlatformMoving(Vector3 displacement) => _model.OnPlatformMoving(displacement);
    public void SetCanMove(bool canMove) => _canMove = canMove;
    #endregion

    #region LASER MANAGEMENT
    public void LaserRecived()
    {
        if (_isDead) return;
        _isDead = true;
        View.SetAnimatorSpeed(0f);
        View.DeathSound();
        _solvingController?.BurnShader();
        HookInputs(false);
    }
    
    public void LaserNotRecived() { }
    #endregion

    #region DESINTEGRATION MANAGEMENT
    public void StartDesintegratePlayer() { _solvingController.StartDesintegrateShader(); }
    public void StopDesintegratePlayer() { _solvingController.StopDesintegrateShader(); }
    public void SetDesintegratePlayer(float a) { _solvingController.SetDesintegrateShader(a); }
    private void OnDissolveCompleted() => StartCoroutine(RespawnPlayer());
    #endregion

    public void SetCheckPointPos(Vector3 newPos)
    {
        _checkPointPos = newPos;
    }

    public IEnumerator RespawnPlayer()
    {
        GlitchDeathController.Instance.TriggerGlitch();
        yield return new WaitForSeconds(1f);

        CC.enabled = false;
        transform.position = _checkPointPos;
        CC.enabled = true;

        yield return new WaitForSeconds(0.5f);

        _isDead = false;
        View.SetAnimatorSpeed(1f);
        _solvingController?.RespawnPlayer();
        _collider.enabled = true;
        HookInputs(true);
    }

    #region TRIGGERS MANAGEMENT
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable)) _interactableHandler.Add(interactable);
        else if (coll.CompareTag("Void"))
        {
            _isDead = true;
            View.FallSound();
            StartCoroutine(RespawnPlayer());
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable)) _interactableHandler.Remove(interactable);
    }
    #endregion
}
