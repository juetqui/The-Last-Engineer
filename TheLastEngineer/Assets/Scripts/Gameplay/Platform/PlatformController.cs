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
    private PlatformStateMachine _fsm;

    // Datos compartidos por los estados
    public float CurrentSpeed { get; private set; }
    public float MoveSpeed => _moveSpeed;
    public float CorruptedMoveSpeed => (_corruptedMoveSpeed > 0f) ? _corruptedMoveSpeed : _moveSpeed * 0.5f;
    public float WaitCD => _waitCD;
    public float WaitTimer { get; set; }

    public IMovablePassenger Passenger => _passenger;
    public PlatformMotor Motor => _motor;
    public RouteManager Route => _route;
    public Vector3 CurrentTarget => _route.CurrentPoint;

    private Coroutine _changingColor = null;

    private void Awake()
    {
        _route = new RouteManager(_positions);
        _motor = new PlatformMotor(transform, null);
        CurrentSpeed = _moveSpeed;
        if (_corruptedMoveSpeed <= 0f) _corruptedMoveSpeed = _moveSpeed / 2f;
    }


    private void Start()
    {
        _connection.OnNodeConnected += OnConnectionChanged;
        
         if (PlayerNodeHandler.Instance != null)
            PlayerNodeHandler.Instance.OnNodeGrabbed += OnNodeGrabbed;

        _fsm = new PlatformStateMachine(this, _connection.StartsConnected);

        SetPositiveFeedback(_connection.StartsConnected);
    }

    private void OnDestroy()
    {
        if (_connection != null)
            _connection.OnNodeConnected -= OnConnectionChanged;

        if (PlayerNodeHandler.Instance != null)
            PlayerNodeHandler.Instance.OnNodeGrabbed -= OnNodeGrabbed;
    }


    private void Update()
    {
        if (!_route.IsValid) return;
        _fsm?.Tick(Time.deltaTime);
    }

    public void SetPositiveFeedback(bool Active)
    {
        //refuerzoPositivo.SetActive(Active);

        if (_changingColor != null) StopCoroutine(_changingColor);

        Color targetColor = Active ? Color.cyan : Color.red;
        _changingColor = StartCoroutine(ChangeColor(targetColor));
    }

    private IEnumerator ChangeColor(Color targetColor)
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
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
            _fsm.ToWaiting();
        else
            _fsm.ToInactive();
    }

    private void OnNodeGrabbed(bool hasNode, NodeType nodeType)
    {
        CurrentSpeed = (hasNode && nodeType == NodeType.Corrupted) ? CorruptedMoveSpeed : MoveSpeed;
    }

    /* -------------------- API interna usada por los estados -------------------- */

    public void AdvanceRouteAndWait()
    {
        _route.Advance();
        _fsm.ToWaiting();
    }

    public void BeginWait()
    {
        WaitTimer = _waitCD;
    }

    public void StopPassenger()
    {
        if (_passenger != null) return;

        _motor.Stop(_passenger);
    }

    public bool ReachedTarget()
    {
        return _motor.InTarget(CurrentTarget);
    }

    public void MoveStep()
    {
        _motor.MoveTowards(CurrentTarget, CurrentSpeed, _passenger);
    }

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