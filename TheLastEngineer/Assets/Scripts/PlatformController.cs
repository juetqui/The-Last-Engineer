using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private Transform[] _positions;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _corrputedMoveSpeed=0;
    [SerializeField] private float _waitCD;
    [SerializeField] private float _corruptedWaitCD=0;
    private float _currentWaitCD;
    [SerializeField] private LayerMask _floorMask;
    [SerializeField] private NodeType _requiredNode = NodeType.Corrupted;
    private IMovablePassenger _passenger = default;
    private Vector3 _targetPos = default;
    private int _index = 0;
    private bool _canMove = false, _arrived = false;
    private Coroutine _waitingToMove = null;

    void Awake()
    {
        _targetPos = _positions[0].position;
        _connection.OnNodeConnected += AvailableToMove;
        if (_corrputedMoveSpeed<=0)
        {
            _corrputedMoveSpeed = _moveSpeed;

        }
        if (_corruptedWaitCD<=0)
        {
            _corruptedWaitCD = _waitCD;
        }
    }

    void Update()
    {
        MovePlatform();
        if (PlayerTDController.Instance.HasNode() && PlayerTDController.Instance.GetCurrentNodeType() == NodeType.Corrupted)
        {
            _currentWaitCD = _corruptedWaitCD;
        }
        else
        {
            _currentWaitCD = _waitCD;
        }
    }

    private void MovePlatform()
    {
        if (!_canMove) return;
        //{
        //    _targetPos = _positions[0].position;

        //    if (Vector3.Distance(transform.position, _targetPos) > 0.1f)
        //        MoveToTarget();
        //}

        if (_canMove && !_arrived && _waitingToMove == null)
        {
            if (Vector3.Distance(transform.position, _targetPos) > 0.1f)
                MoveToTarget();
            else
            {
                if (_passenger != null)
                    _passenger.OnPlatformMoving(Vector3.zero);
                
                _index++;

                if (_index == _positions.Length)
                    _index = 0;

                _waitingToMove = StartCoroutine(WaitToNextTarget());
                _targetPos = _positions[_index].position;
            }
        }
    }

    private void MoveToTarget()
    {
        Vector3 dir = _targetPos - transform.position;
        Vector3 displacement;
        if (PlayerTDController.Instance.HasNode() && PlayerTDController.Instance.GetCurrentNodeType() == NodeType.Corrupted)
        {
            displacement = dir.normalized * Time.deltaTime * _corrputedMoveSpeed;
        }
        else
        {
            displacement = dir.normalized * Time.deltaTime * _moveSpeed;
        }
        transform.position += displacement;

        if (_passenger != null)
        {
            _passenger.OnPlatformMoving(displacement);
        }
    }

    public void AvailableToMove(NodeType node, bool isActive)
    {
        if (node == _requiredNode && isActive)
        {
            _canMove = true;
        }
        else
        {
            _canMove = false;
        }
    }

    private IEnumerator WaitToNextTarget()
    {
        _arrived = true;
        float cDpassed = 1;
        while (cDpassed > 0)
        {
            yield return new WaitForSeconds(_currentWaitCD*0.2f);
            cDpassed = cDpassed - 0.2f;
        }
        _arrived = false;
        _waitingToMove = null;
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
        if (coll.TryGetComponent(out IMovablePassenger passenger) && _passenger == passenger)
        {
            if (_passenger != null)
            {
                _passenger.OnPlatformMoving(Vector3.zero);
                _passenger = null;
            }
        }
    }
}
