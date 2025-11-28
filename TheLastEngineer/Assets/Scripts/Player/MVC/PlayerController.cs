using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour, IMovablePassenger, ILaserReceptor
{
    #region Variables
    public static PlayerController Instance = null;


    [Header("Data & MVC")]
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private ParticleSystem _walkPS, _orbitPS, _defaultPS, _corruptedPS, _teleportPS;
    [SerializeField] private AudioSource _walkSource, _fxSource;
    [SerializeField] private Camera _mainCam;

    public CharacterController CC { get; private set; }
    private Collider _collider = default;
    private Animator _animator = default;

    public Action<float, float> OnDash;
    public Action<IInteractable> OnInteractableSelected;
    public Action OnPlayerFell;
    public Action OnDied;
    public Action OnRespawned;
    public Action<Glitcheable> OnGlitcheableInArea;
    public Action<float> OnDissolving;
    public Action OnTeleported;

    // --- Internals
    private PlayerModel _model;
    public PlayerView View { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    
    private InputHandler _input;
    private GlitcheableDetector _glitcheableDetector;
    private PlayerNodeHandler _nodeHandler;
    private CinemachineImpulseSource _impulse;
    private InteractableHandler _interactableHandler;

    private Vector2 _move = Vector2.zero;
    private float _currentSpeed;
    private bool _isDead = false, _canMove = true;

    private Vector3 _checkPointPos;
    private Vector3 _teleportPos;

    public bool IsDead { get { return _isDead; } }
    public Vector3 TeleportPos {  get { return _teleportPos; } }
    #endregion

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
        _glitcheableDetector = new GlitcheableDetector(10f, _playerData.glitchDetectionLayer);
        _nodeHandler = GetComponent<PlayerNodeHandler>();
        _impulse = GetComponent<CinemachineImpulseSource>();
        _interactableHandler = new InteractableHandler();

        _model = new PlayerModel(CC, transform, _playerData, _collider);
        View = new PlayerView(_renderer, _walkPS, _orbitPS, _animator, _walkSource, _fxSource, _playerData, _defaultPS, _corruptedPS, _teleportPS);

        _checkPointPos = transform.position;
    }

    private void Start()
    {
        _currentSpeed = _playerData.moveSpeed;
        View.OnStart();

        StateMachine = new PlayerStateMachine(this, _nodeHandler);

        HookInputs(true);
        
        OnDied += _input.DisableInputs;
        OnRespawned += _input.EnableInputs;
    }

    private void Update()
    {
        var mv3 = GetMovement3D();
        _model.OnUpdate(mv3, _mainCam.transform.forward, _mainCam.transform.right, _currentSpeed);
        View.Walk(mv3);
        StateMachine.Tick();

        GetClosestGlitcheable();
    }

    private void OnDestroy()
    {
        HookInputs(false);
    }

    #region INPUTS MANAGEMENT
    private void HookInputs(bool enable)
    {
        if (_input == null) return;

        if (enable)
        {
            _input.OnMove += OnMove;
            _input.OnDash += OnDashPressed;
            _input.OnInteractStart += OnInteractPressed;
            _input.OnInteractCancel += OnInteractCanceled;
            _input.OnCancelSelect += OnCancelSelect;
        }
        else
        {
            _input.OnMove -= OnMove;
            _input.OnDash -= OnDashPressed;
            _input.OnInteractStart -= OnInteractPressed;
            _input.OnInteractCancel -= OnInteractCanceled;
            _input.OnCancelSelect -= OnCancelSelect;
        }
    }
    private void OnMove(Vector2 mv) => _move = _isDead || !_canMove ? Vector2.zero : mv;
    private void OnDashPressed()
    {
        if (_model.CanDashWithCoyoteTime() && !_isDead)
        {
            InputManager.Instance.RumblePulse(
                _playerData.lowRumbleFrequency,
                _playerData.highRumbleFrequency,
                _playerData.rumbleDuration);

            Vector3 dashInput = GetMovement3D();
            if (dashInput == Vector3.zero)
                dashInput = transform.forward;

            Vector3 camForward = _mainCam.transform.forward;
            Vector3 camRight = _mainCam.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 dashDir = (camForward * dashInput.z + camRight * dashInput.x).normalized;

            StartCoroutine(_model.Dash(dashDir));

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

        if (target == null)
            View.PlayErrorSound(_playerData.emptyHand);

        StateMachine.CurrentState?.HandleInteraction(target);
        OnInteractableSelected?.Invoke(target);
    }

    private void OnInteractCanceled()
    {
        StateMachine.CurrentState?.Cancel();
    }

    private void OnCancelSelect()
    {
        if (!InspectionSystem.Instance.CanRotate) return;

        OnInteractableSelected?.Invoke(null);
    }

    private Vector3 GetMovement3D()
    {
        if (_isDead || !_canMove) return Vector3.zero;
        
        return new Vector3(_move.x, 0f, _move.y);
    }

    private void CheckInteractionOutcome(Glitcheable g)
    {
        View.PlayErrorSound(_playerData.emptyHand);
    }

    public bool CheckForWalls()
    {
        Vector3 rayPos = new(transform.position.x, transform.position.y + 2f, transform.position.z);
        return Physics.Raycast(rayPos, transform.forward, _playerData.maxWallDist, _playerData.wallMask);
    }
    #endregion

    #region STATE MACHINE API
    public void PickUpNode(NodeController node) => _nodeHandler.Pick(node);
    public void ReleaseNode() => _nodeHandler.Release();
    public void DropNode() => _nodeHandler.Release(true);
    public float GetHoldInteractionTime() => _playerData.holdInteractionTime;
    public void AddInteractable(IInteractable interactable) => _interactableHandler.Add(interactable);
    public void RemoveInteractable(IInteractable interactable) => _interactableHandler.Remove(interactable);
    public void SetPos(Vector3 targetPos) => _model.SetPos(targetPos);
    public void SetTeleport(Vector3 targetPos) => _teleportPos = targetPos;
    public void GetClosestGlitcheable()
    {
        Glitcheable nearest = _interactableHandler.GetClosestGlitcheable(transform.position);
        OnGlitcheableInArea?.Invoke(nearest);
    }
    public void Dissolving(float timer) => OnDissolving?.Invoke(timer);
    public void SetCollisions(bool setCollider)
    {
        _canMove = setCollider;
        _model.SetGravity(setCollider);

        if (setCollider)
        {
            _input.EnableInputs();
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(_playerData.defaultLayer.value, 2));
        }
        else
        {
            _input.DisableInputs();
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(_playerData.teleportLayer.value, 2));
        }
    }
    public void StartTeleport()
    {
        _model.StartTeleport(_teleportPos, 0.25f);
    }
    public void Teleport()
    {
        if (_model.Teleport())
            OnTeleported?.Invoke();
    }
    public void PlayTeleportPS() => View.TeleportPS();
    #endregion

    #region PLATFORM TP MANAGEMENT
    public void OnPlatformMoving(Vector3 displacement) => _model.OnPlatformMoving(displacement);
    public void SetCanMove(bool canMove) => _canMove = canMove;
    #endregion

    #region LASER MANAGEMENT
    public void LaserRecived()
    {
        if (_isDead) return;
        View.SetAnimatorSpeed(0f);
        //_solvingController?.BurnShader();
        StartCoroutine(RespawnPlayer(CauseOfDeath.Laser));
    }
    
    public void LaserNotRecived() { }
    #endregion

    #region CheckPoint y Respawn
    public void SetCheckPointPos(Vector3 newPos)
    {
        _checkPointPos = newPos;
    }

    public IEnumerator RespawnPlayer(CauseOfDeath cause)
    {
        _isDead = true;
        OnDied?.Invoke();
        _interactableHandler.Clear();

        if (cause == CauseOfDeath.Laser)
            View.DeathSound();
        else if (cause == CauseOfDeath.Fall)
        {
            View.FallSound();
            yield return new WaitForSeconds(1f);
            OnPlayerFell?.Invoke();
        }

        GlitchDeathController.Instance.TriggerGlitch();
        yield return new WaitForSeconds(1f);

        _model.SetRespawnPos(_checkPointPos);

        yield return new WaitForSeconds(0.5f);

        _isDead = false;
        View.SetAnimatorSpeed(1f);
        //_solvingController?.RespawnPlayer();
        _collider.enabled = true;
        OnRespawned?.Invoke();
    }
    #endregion

    public void WalkSound()
    {
        if (!CC.isGrounded) return;

        View.WalkSound();
    }

    #region TRIGGERS MANAGEMENT
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
            _interactableHandler.Add(interactable);
        else if (coll.CompareTag("Void") && !_isDead)
        {
            StartCoroutine(RespawnPlayer(CauseOfDeath.Fall));
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
            _interactableHandler.Remove(interactable);
    }
    #endregion
}
public enum CauseOfDeath
{
    Teleport,
    Fall,
    Laser
}
