using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour, IMovablePassenger, ILaserReceptor
{
    #region Variables
    public static PlayerController Instance = null;


    [Header("Data & MVC")]
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private ParticleSystem _walkPS, _orbitPS, _defaultPS, _corruptedPS, _teleportPS;
    [SerializeField] private AudioSource _walkSource, _fxSource;
    [SerializeField] private FollowController _followController;
    [SerializeField] private PlayerInteractionDetector _interactionDetector;
    
    [Header("Debug")]
    [SerializeField] private bool debug = false;

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

    public Action<IInteractable> OnInteractableDetected;

    // Cinematic Events
    public Action<Transform, LayerMask, bool> OnCinematicSetupRequested; // parent, cinematicLayer, storeCCState
    public Action<LayerMask, bool> OnCinematicRestoreRequested; // defaultLayer, ccWasEnabled

    // --- Internals
    private PlayerModel _model;
    public PlayerView View { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    
    private InputHandler _input;
    private PlayerNodeHandler _nodeHandler;
    private CinemachineImpulseSource _impulse;
    private InteractableHandler _interactableHandler;
    private LineOfSightChecker _obstruction;

    private Vector2 _move = Vector2.zero;
    private float _currentSpeed;
    private bool _isDead = false, _canMove = true;

    private Vector3 _checkPointPos;
    private Vector3 _teleportPos;
    private Glitcheable _lastNearestGlitcheable;

    public bool IsDead { get { return _isDead; } }
    public Vector3 TeleportPos {  get { return _teleportPos; } }
    public PlayerNodeHandler NodeHandler => _nodeHandler;
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        CC = GetComponent<CharacterController>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        
        _input = GetComponent<InputHandler>();
        _nodeHandler = GetComponent<PlayerNodeHandler>();
        _impulse = GetComponent<CinemachineImpulseSource>();

        _obstruction = new LineOfSightChecker(_playerData.wallMask, _playerData.losHeightOffset, _playerData.losEndMargin);
        _interactableHandler = new InteractableHandler();

        // El detector es el dueño del LOS: lo recalcula por frame y publica el resultado en el
        // handler, que solo cachea el flag.
        if (_interactionDetector != null)
            _interactionDetector.Initialize(_interactableHandler, this, _obstruction,
                _playerData.interactionRadius, _playerData.losCheckInterval, _playerData.losDebounceTime);

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

        if (LevelLoader.Instance != null)
            LevelLoader.Instance.OnLoading += _input.DisableInputs;

        // Subscribe to cinematic events
        OnCinematicSetupRequested += HandleCinematicSetup;
        OnCinematicRestoreRequested += HandleCinematicRestore;

        // Subscribe to CinematicManager events for state transitions
        if (CinematicManager.Instance != null)
        {
            CinematicManager.Instance.OnRequestEnterCinematicState += HandleEnterCinematicState;
            CinematicManager.Instance.OnRequestExitCinematicState += HandleExitCinematicState;
        }
    }

    private void Update()
    {
        var mv3 = GetMovement3D();
        _followController.GetCameraBasis(out var camForward, out var camRight);
        var actualMovement = _model.OnUpdate(mv3, camForward, camRight, _currentSpeed);

        View.Walk(actualMovement);
        StateMachine.Tick();

        GetClosestGlitcheable();
        
        var target = _interactableHandler.GetInteractable(_nodeHandler, transform.position);
        OnInteractableDetected?.Invoke(target);
    }

    private void OnDestroy()
    {
        _model?.StopTweens();

        _lastNearestGlitcheable = null;
        HookInputs(false);
        
        // Unsubscribe from cinematic events
        OnCinematicSetupRequested -= HandleCinematicSetup;
        OnCinematicRestoreRequested -= HandleCinematicRestore;

        // Unsubscribe from CinematicManager events
        if (CinematicManager.Instance != null)
        {
            CinematicManager.Instance.OnRequestEnterCinematicState -= HandleEnterCinematicState;
            CinematicManager.Instance.OnRequestExitCinematicState -= HandleExitCinematicState;
        }
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

            if (debug) _input.OnDebug += OnDebug;
        }
        else
        {
            _input.OnMove -= OnMove;
            _input.OnDash -= OnDashPressed;
            _input.OnInteractStart -= OnInteractPressed;
            _input.OnInteractCancel -= OnInteractCanceled;
            _input.OnCancelSelect -= OnCancelSelect;

            if (debug) _input.OnDebug -= OnDebug;
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

            var dashInput = GetMovement3D();
            var dashDir = Vector3.zero;

            if (dashInput == Vector3.zero)
            {
                dashInput = transform.forward;
            }
            else
            {
                _followController.GetCameraBasis(out var camForward, out var camRight);
                dashDir = (camForward * dashInput.z + camRight * dashInput.x).normalized;
            }

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

    private void OnDebug() => LevelLoader.Instance.PerformDebug();

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

    // Delega en el detector: es el único que puede sacar el interactuable de la lista Y del set
    // de colliders solapados a la vez. Sacarlo solo del handler lo deja imposible de re-registrar.
    public void RemoveInteractable(IInteractable interactable)
    {
        if (_interactionDetector != null) _interactionDetector.Forget(interactable);
        else _interactableHandler.Remove(interactable);
    }

    // Re-anuncia un interactuable que se movió o reactivó su collider dentro del radio.
    public void RescanInteractable(IInteractable interactable, Collider coll)
        => _interactionDetector?.Rescan(interactable, coll);

    // Estado cacheado por el detector: sigue en rango y con línea de visión despejada. Lo consultan
    // los estados del jugador para cortar una interacción en curso si aparece una pared en el medio
    // o si el objetivo se va (el Glitcheable se mueve mientras se lo mantiene apretado).
    public bool IsInteractableAvailable(IInteractable interactable)
        => _interactableHandler == null || _interactableHandler.IsSelectable(interactable);
    public void SetPos(Vector3 targetPos) => _model.SetPos(targetPos);
    public void SetTeleport(Vector3 targetPos) => _teleportPos = targetPos;
    public void GetClosestGlitcheable()
    {
        var nearest = _interactableHandler.GetClosestGlitcheable(transform.position);

        if (nearest == _lastNearestGlitcheable) return;

        _lastNearestGlitcheable = nearest;
        OnGlitcheableInArea?.Invoke(nearest);
    }
    public void Dissolving(float timer) => OnDissolving?.Invoke(timer);
    public void SetCinematicMovement(Vector3 direction) => _model.SetCinematicMovement(direction);
    public void ClearCinematicMovement() => _model.ClearCinematicMovement();
    public bool IsInCinematicMode() => _model.IsInCinematicMode();
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

    #region CINEMATIC MANAGEMENT
    private void HandleCinematicSetup(Transform parentObject, LayerMask cinematicLayer, bool storeCCState)
    {
        if (parentObject != null)
            transform.SetParent(parentObject);

        if (storeCCState) CC.enabled = false;

        var cinematicLayerIndex = Mathf.RoundToInt(Mathf.Log(cinematicLayer.value, 2));
        gameObject.layer = cinematicLayerIndex;
    }

    private void HandleCinematicRestore(LayerMask defaultLayer, bool ccWasEnabled)
    {
        transform.SetParent(null);
        CC.enabled = ccWasEnabled;

        var defaultLayerIndex = Mathf.RoundToInt(Mathf.Log(defaultLayer.value, 2));
        gameObject.layer = defaultLayerIndex;
    }

    private void HandleEnterCinematicState()
    {
        StateMachine.TransitionToCinematicState();
    }

    private void HandleExitCinematicState()
    {
        StateMachine.TransitionFromCinematicState();
    }
    #endregion

    #region PLATFORM TP MANAGEMENT
    public void OnPlatformMoving(Vector3 displacement) => _model.OnPlatformMoving(displacement);
    public void SetCanMove(bool canMove) => _canMove = canMove;
    #endregion

    #region LASER MANAGEMENT
    public void LaserReceived()
    {
        if (_isDead) return;
        View.SetAnimatorSpeed(0f);
        //_solvingController?.BurnShader();
        StartCoroutine(RespawnPlayer(CauseOfDeath.Laser));
    }
    
    public void LaserNotReceived() { }
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
        // Sin esto el set de solapados queda con entradas fantasma y cualquier interactuable que
        // siga dentro del radio al reaparecer no vuelve a entrar a la lista nunca.
        if (_interactionDetector != null) _interactionDetector.ResetTracking();

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
        // El registro de interactuables lo maneja PlayerInteractionDetector (trigger fijo del jugador).
        if (coll.CompareTag("Void") && !_isDead)
            StartCoroutine(RespawnPlayer(CauseOfDeath.Fall));
    }
    #endregion

    #region DEBUG GIZMOS
    // Dibuja una línea desde el jugador hasta cada interactuable registrado en la lista.
    // Verde = línea de visión despejada; rojo = bloqueada. Lee el flag CACHEADO que publica el
    // detector, no un raycast propio: así el gizmo muestra el mismo estado (debounce incluido)
    // que decide la selección, y no puede mentir respecto de lo que pasa en runtime.
    private void OnDrawGizmos()
    {
        if (!debug || _interactableHandler == null || _obstruction == null) return;

        Vector3 offset = Vector3.up * _obstruction.HeightOffset;
        Vector3 origin = transform.position + offset;

        foreach (var interactable in _interactableHandler.Interactables)
        {
            if (interactable == null) continue;

            Vector3 target = interactable.Transform.position + offset;

            Gizmos.color = _interactableHandler.HasLineOfSight(interactable) ? Color.green : Color.red;
            Gizmos.DrawLine(origin, target);
            Gizmos.DrawWireSphere(target, 0.15f);
        }
    }
    #endregion
}
public enum CauseOfDeath
{
    Teleport,
    Fall,
    Laser
}
