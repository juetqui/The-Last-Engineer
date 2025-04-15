using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private Transform[] _positions;
    [SerializeField] private BoxCollider _topCollider;
    [SerializeField] private BoxCollider _bottomCollider;
    [SerializeField] private BoxCollider _leftCollider;
    [SerializeField] private BoxCollider _rightCollider;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _waitCD;
    [SerializeField] private LayerMask _floorMask;

    private NodeType _requiredNode = NodeType.Blue;
    private PlayerTDController _player = default;
    private IMovablePassenger _passenger = default;
    private Vector3 _targetPos = default;
    private int _index = 0;
    private bool _canMove = false, _arrived = false;

    private bool CheckTop() => Physics.Raycast(transform.position, transform.forward, 3.25f, _floorMask);
    private bool CheckBottom() => Physics.Raycast(transform.position, -transform.forward, 3.25f, _floorMask);
    private bool CheckLeft() => Physics.Raycast(transform.position, -transform.right, 3.25f, _floorMask);
    private bool CheckRight() => Physics.Raycast(transform.position, transform.right, 3.25f, _floorMask);

    void Awake()
    {
        _targetPos = _positions[0].position;
        _connection.OnNodeConnected += AvailableToMove;
    }

    private void Start()
    {
        CheckColliders();
    }

    void Update()
    {
        MovePlatform();
        CheckColliders();
    }

    private void MovePlatform()
    {
        if (!_canMove)
        {
            _targetPos = _positions[0].position;

            if (Vector3.Distance(transform.position, _targetPos) > 0.01f)
                MoveToTarget();
        }

        if (_canMove && !_arrived)
        {
            if (Vector3.Distance(transform.position, _targetPos) > 0.01f)
                MoveToTarget();
            else
            {
                if (_passenger != null)
                    _passenger.OnPlatformMoving(Vector3.zero);
                
                _index++;

                if (_index == _positions.Length)
                    _index = 0;

                StartCoroutine(WaitToNextTarget());
                _targetPos = _positions[_index].position;
            }
        }
    }

    private void MoveToTarget()
    {
        Vector3 dir = _targetPos - transform.position;
        Vector3 displacement = dir.normalized * Time.deltaTime * _moveSpeed;
        transform.position += displacement;

        if (_player != null && _player is IMovablePassenger passenger)
        {
            _passenger = passenger;
            _passenger.OnPlatformMoving(displacement);
        }
    }

    public void AvailableToMove(NodeType node, bool isActive)
    {
        if (node == _requiredNode && isActive || node == NodeType.Dash && isActive)
        {
            _canMove = true;
        }
        else
        {
            _canMove = false;
        }
    }

    private void CheckColliders()
    {
        if (CheckTop())
            _topCollider.enabled = false;
        else
            _topCollider.enabled = true;

        if (CheckBottom())
            _bottomCollider.enabled = false;
        else
            _bottomCollider.enabled = true;

        if (CheckLeft())
            _leftCollider.enabled = false;
        else
            _leftCollider.enabled = true;

        if (CheckRight())
            _rightCollider.enabled = false;
        else
            _rightCollider.enabled = true;
    }

    private IEnumerator WaitToNextTarget()
    {
        _arrived = true;
        yield return new WaitForSeconds(_waitCD);
        _arrived = false;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent<PlayerTDController>(out PlayerTDController player))
        {
            _player = player;
            _player.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent<PlayerTDController>(out PlayerTDController player) && _player == player)
        {
            if (_passenger != null)
            {
                _passenger.OnPlatformMoving(Vector3.zero);
                _passenger = null;
            }
            
            _player.transform.SetParent(null);
            _player = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, transform.forward * 3.25f);
        Gizmos.DrawRay(transform.position, -transform.forward * 3.25f);
        Gizmos.DrawRay(transform.position, -transform.right * 3.25f);
        Gizmos.DrawRay(transform.position, transform.right * 3.25f);
    }
}
