using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform[] _positions;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _corruptedMoveSpeed = 0f;
    [SerializeField] private float _waitCD = 1f;
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    [SerializeField] private Connection _connection = default;

    // --- Runtime
    private RouteManager _route;
    private PlatformMotor _motor;
    private IMovablePassenger _passenger;

    // Estado actual y objetos-estado (cacheados para evitar GC)
    private IPlatformState _state;
    private readonly InactiveState _inactiveState = new InactiveState();
    private readonly WaitingState _waitingState = new WaitingState();
    private readonly MovingState _movingState = new MovingState();

    // Datos compartidos por los estados
    internal float CurrentSpeed { get; private set; }
    internal float MoveSpeed => _moveSpeed;
    internal float CorruptedMoveSpeed => (_corruptedMoveSpeed > 0f) ? _corruptedMoveSpeed : _moveSpeed * 0.5f;
    internal float WaitCD => _waitCD;
    internal float WaitTimer { get; set; }

    internal IMovablePassenger Passenger => _passenger;
    internal PlatformMotor Motor => _motor;
    internal RouteManager Route => _route;
    internal Vector3 CurrentTarget => _route.CurrentPoint;

    [SerializeField] protected GameObject refuerzoPositivo;

    private Coroutine _changingColor = null;

    private void Awake()
    {
        _route = new RouteManager(_positions);
        _motor = new PlatformMotor(transform, null);   // INSTANCIAR EL MOTOR
        CurrentSpeed = _moveSpeed;
        if (_corruptedMoveSpeed <= 0f) _corruptedMoveSpeed = _moveSpeed / 2f;
    }


    private void Start()
    {
        // Subscripción a cambios de conexión
        _connection.OnNodeConnected += OnConnectionChanged;
        
         // Velocidad según si el jugador porta un nodo corrupto (opcional)
         if (PlayerTDController.Instance != null)
            PlayerTDController.Instance.OnNodeGrabed += OnNodeGrabbed;
        
         // Estado inicial
        bool canMove = _connection.StartsConnected; // y si hay filtro por tipo: && _connection.CurrentType == _requiredNode;
        SetState(canMove ? _waitingState : _inactiveState);
        SetPositiveFeedback(canMove);
    }

    private void OnDestroy()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= OnConnectionChanged;

        if (PlayerTDController.Instance != null)
            PlayerTDController.Instance.OnNodeGrabed -= OnNodeGrabbed;
    }


    private void Update()
    {
        if (!_route.IsValid) return;
        _state?.Tick(this);
    }


    public void SetPositiveFeedback(bool Active)
    {
        refuerzoPositivo.SetActive(Active);

        if (_changingColor != null) StopCoroutine(_changingColor);

        Color targetColor = Active ? Color.cyan : Color.red;
        _changingColor = StartCoroutine(ChangeColor(targetColor));
    }

    private IEnumerator ChangeColor(Color targetColor)
    {
        Renderer renderer = GetComponent<Renderer>();
        float counter = 0f;

        while (counter < 1f)
        {
            counter += Time.deltaTime * 0.05f;

            Color currentColor = renderer.material.GetColor("_EmissiveColor");
            Color newColor = Color.Lerp(currentColor, targetColor, counter);

            renderer.material.SetColor("_EmissiveColor", newColor);
            yield return null;
        }

        renderer.material.SetColor("_EmissiveColor", targetColor);
        _changingColor = null;
    }


    /* -------------------- Eventos externos -------------------- */
    private void OnConnectionChanged(NodeType type, bool active)
    {
        bool canMove = (type == _requiredNode) && active;
        SetPositiveFeedback(canMove);

        if (canMove)
            SetState(_waitingState);
        else
            SetState(_inactiveState);
    }

    private void OnNodeGrabbed(bool hasNode, NodeType nodeType)
    {
        CurrentSpeed = (hasNode && nodeType == NodeType.Corrupted) ? CorruptedMoveSpeed : MoveSpeed;
    }

    /* -------------------- API interna usada por los estados -------------------- */

    internal void AdvanceRouteAndWait()
    {
        _route.Advance();
        SetState(_waitingState);
    }

    internal void BeginWait()
    {
        WaitTimer = _waitCD;
    }

    internal void StopPassenger()
    {
        if (_passenger != null) _motor.Stop(_passenger);
    }

    internal bool ReachedTarget()
    {
        return _motor.InTarget(CurrentTarget);
    }

    internal void MoveStep()
    {
        _motor.MoveTowards(CurrentTarget, CurrentSpeed, _passenger);
    }

    internal void SetState(IPlatformState next)
    {
        if (_state == next) return;
        _state?.Exit(this);
        _state = next;
        _state.Enter(this);
    }

    /* -------------------- Trigger pasajero -------------------- */

    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out IMovablePassenger passenger))
            _passenger = passenger;
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.TryGetComponent(out IMovablePassenger passenger) && _passenger == passenger)
        {
            _passenger.OnPlatformMoving(Vector3.zero);
            _passenger = null;
        }
    }
}

/* ========================================================================== */
/*                              Patrón de Estados                              */
/* ========================================================================== */

public interface IPlatformState
{
    void Enter(PlatformController ctx);
    void Tick(PlatformController ctx);
    void Exit(PlatformController ctx);
}

public class InactiveState : IPlatformState
{
    public void Enter(PlatformController ctx)
    {
        ctx.WaitTimer = 0f;
        ctx.StopPassenger(); // asegura desplazamiento cero
    }

    public void Tick(PlatformController ctx)
    {
        // No hace nada mientras el cable/condición no habilite
        // El cambio a Waiting lo dispara el evento OnConnectionChanged
    }

    public void Exit(PlatformController ctx) { }
}

public class WaitingState : IPlatformState
{
    public void Enter(PlatformController ctx)
    {
        // Reinicia la cuenta
        ctx.BeginWait();
        ctx.StopPassenger(); // por si venía de Moving
    }

    public void Tick(PlatformController ctx)
    {
        if (ctx.WaitTimer > 0f)
        {
            ctx.WaitTimer -= Time.deltaTime;
            if (ctx.WaitTimer <= 0f)
            {
                ctx.SetState(new MovingState()); // O usar instancia cacheada si preferís (ver nota abajo)
            }
        }
    }

    public void Exit(PlatformController ctx) { }
}

public class MovingState : IPlatformState
{
    public void Enter(PlatformController ctx)
    {
        // Nada especial; el target se evalúa cada frame desde RouteManager
    }

    public void Tick(PlatformController ctx)
    {
        if (ctx.ReachedTarget())
        {
            ctx.StopPassenger();
            ctx.AdvanceRouteAndWait(); // Avanza al próximo punto y vuelve a esperar
            return;
        }

        ctx.MoveStep();
    }

    public void Exit(PlatformController ctx) { }
}
