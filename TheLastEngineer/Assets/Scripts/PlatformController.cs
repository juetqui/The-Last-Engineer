using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private Transform[] _positions;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _corrputedMoveSpeed = 0;
    [SerializeField] private float _waitCD;
    [SerializeField] private LayerMask _floorMask;
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    
    private IMovablePassenger _passenger = default;
    private Vector3 _targetPos = default;
    private float _currentSpeed = default;
    private int _index = 0;
    private bool _canMove = false, _isMoving = false;
    private Coroutine _waitingToMove = null;

    private void Start()
    {
        _targetPos = _positions[0].position;
        _currentSpeed = _moveSpeed;
        
        _connection.OnNodeConnected += AvailableToMove;
        PlayerTDController.Instance.OnNodeGrabed += CheckSpeed;

        if (_corrputedMoveSpeed <= 0f) _corrputedMoveSpeed = _moveSpeed / 2f;

        _currentSpeed = _moveSpeed;
        CheckSpeed(false, NodeType.None);

        AvailableToMove(_requiredNode, _connection.StartsConnected);
    }

    private void AvailableToMove(NodeType node, bool isActive)
    {
        _canMove = (node == _requiredNode && isActive);
        if (node == _requiredNode && isActive)
        {
            _connection.SetPositiveFeedback(true);

        }
        else
        {
            _connection.SetPositiveFeedback(false);

        }
        if (_canMove && _waitingToMove == null)
        {
            _waitingToMove = StartCoroutine(WaitToNextTarget());
        }
        else if (!_canMove && _waitingToMove != null)
        {
            StopCoroutine(_waitingToMove);
            _waitingToMove = null;
        }
    }

    private void CheckSpeed(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType != NodeType.Corrupted)
        {
            _currentSpeed = _moveSpeed;
            return;
        }
        
        _currentSpeed = _corrputedMoveSpeed;
    }

    private void MovePlatform()
    {
        if (!_canMove || _isMoving) return;
        //{
        //    _targetPos = _positions[0].position;
        //    StartCoroutine(MoveToTarget());
        //}

        StartCoroutine(MoveToTarget());
    }

    private void Move()
    {
        Vector3 dir = _targetPos - transform.position;

        Vector3 displacement = dir.normalized * Time.deltaTime * _currentSpeed;
        transform.position += displacement;

        if (_passenger != null) _passenger.OnPlatformMoving(displacement);
    }

    private IEnumerator MoveToTarget()
    {
        const float threshold = 0.15f;
        
        while (Vector3.Distance(transform.position, _targetPos) > threshold)
        {
            Move();
            yield return null;
        }

        if (_passenger != null)
            _passenger.OnPlatformMoving(Vector3.zero);

        _index = (_index + 1) % _positions.Length;
        _waitingToMove = StartCoroutine(WaitToNextTarget());
    }

    private IEnumerator WaitToNextTarget()
    {
        _targetPos = _positions[_index].position;

        yield return new WaitForSeconds(_waitCD);

        _waitingToMove = null;
        MovePlatform();
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger))
        {
            _passenger = passenger;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IMovablePassenger passenger) && _passenger != null && _passenger == passenger)
        {
            _passenger.OnPlatformMoving(Vector3.zero);
            _passenger = null;
        }
    }
}
