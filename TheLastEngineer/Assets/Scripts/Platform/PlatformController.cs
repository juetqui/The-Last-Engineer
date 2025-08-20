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
    [SerializeField] private GenericConnectionController _connection = default;

    private RouteManager _route = default;
    private PlatformMotor _motor = default;

    private Coroutine _moveRoutine = default;
    private Coroutine _waitRoutine = default;

    private IMovablePassenger _passenger = null;
    private float _currentSpeed = default;

    private void Awake()
    {
        _route = new RouteManager(_positions);

        if (_corruptedMoveSpeed <= 0f) _corruptedMoveSpeed = _moveSpeed / 2f;
        _currentSpeed = _moveSpeed;
    }

    private void Start()
    {
        _motor = new PlatformMotor(transform, null);

        _connection.OnNodeConnected += OnConnectionChanged;
        PlayerTDController.Instance.OnNodeGrabed += OnNodeGrabbed;

        OnConnectionChanged(_requiredNode, _connection.StartsConnected);
    }

    private void OnDestroy()
    {
        _connection.OnNodeConnected -= OnConnectionChanged;
        PlayerTDController.Instance.OnNodeGrabed -= OnNodeGrabbed;
    }

    /* -------------------- EVENTOS -------------------- */

    private void OnConnectionChanged(NodeType type, bool active)
    {
        bool canMove = type == _requiredNode && active;
        
        _connection.SetPositiveFeedback(canMove);

        if (canMove && _waitRoutine == null) _waitRoutine = StartCoroutine(WaitAndMoveRoutine());
        else
        {
            StopAndNull(ref _waitRoutine);
            StopAndNull(ref _moveRoutine);
        }
    }

    private void OnNodeGrabbed(bool hasNode, NodeType nodeType)
    {
        _currentSpeed = (hasNode && nodeType == NodeType.Corrupted) ? _corruptedMoveSpeed : _moveSpeed;
    }

    /* -------------------- RUTINAS -------------------- */

    private IEnumerator WaitAndMoveRoutine()
    {
        yield return new WaitForSeconds(_waitCD);
        _waitRoutine = null;

        if (_moveRoutine == null)
            _moveRoutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        Vector3 target = _route.CurrentPoint;
        _motor = new PlatformMotor(transform, _passenger);

        while (!_motor.InTarget(target))
        {
            _motor.MoveTowards(target, _currentSpeed, _passenger);
            yield return null;
        }

        if (_passenger != null) _motor.Stop(_passenger);

        _route.Advance();
        _moveRoutine = null;

        if (_waitRoutine == null)
            _waitRoutine = StartCoroutine(WaitAndMoveRoutine());
    }

    /* -------------------- UTIL -------------------- */

    private void StopAndNull(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    /* -------------------- PASAJERO -------------------- */

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
