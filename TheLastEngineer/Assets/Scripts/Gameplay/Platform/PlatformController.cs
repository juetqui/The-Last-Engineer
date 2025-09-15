using System.Collections;
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

    private IMovablePassenger _passenger;
    private IPlatformState _state;
    private PlatformStateMachine _fsm;
    private Coroutine _changingColor = null;

    public float CurrentSpeed { get; private set; }
    public float MoveSpeed => _moveSpeed;
    public float CorruptedMoveSpeed => (_corruptedMoveSpeed > 0f) ? _corruptedMoveSpeed : _moveSpeed * 0.5f;
    public float WaitCD => _waitCD;
    public float WaitTimer { get; set; }

    public IMovablePassenger Passenger => _passenger;
    public PlatformMotor Motor { get; private set; }
    public RouteManager Route { get; private set; }
    public Vector3 CurrentTarget => Route.CurrentPoint;

    private void Awake()
    {
        Route = new RouteManager(_positions);
        Motor = new PlatformMotor(transform, null);
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
        if (!Route.IsValid) return;
        _fsm?.Tick(Time.deltaTime);
    }

    public void SetPositiveFeedback(bool Active)
    {
        //refuerzoPositivo.SetActive(Active);

        if (_changingColor != null) StopCoroutine(_changingColor);

        Color targetColor = Active ? Color.cyan : Color.black;
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
            _fsm.ToReturning();
    }

    private void OnNodeGrabbed(bool hasNode, NodeType nodeType)
    {
        CurrentSpeed = (hasNode && nodeType == NodeType.Corrupted) ? CorruptedMoveSpeed : MoveSpeed;
    }

    /* -------------------- API interna usada por los estados -------------------- */

    public void AdvanceRouteAndWait()
    {
        if (Route.HasToWait())
            _fsm.ToWaiting();

        Route.Advance();
    }

    public void BeginWait()
    {
        WaitTimer = _waitCD;
    }

    public void StopPassenger()
    {
        if (_passenger != null) return;

        Motor.Stop(_passenger);
    }

    public bool ReachedTarget()
    {
        return Motor.InTarget(CurrentTarget);
    }

    public void MoveStep()
    {
        Motor.MoveTowards(CurrentTarget, CurrentSpeed, _passenger);
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