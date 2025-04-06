using System.Collections;
using System.Linq;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    //[SerializeField] private PlatformActivator _activator;
    [SerializeField] private SecondaryTM _secTM;
    [SerializeField] private Transform[] _positions;
    [SerializeField] private float _moveSpeed;

    private Vector3 _targetPos = default;
    private int _index = 0;
    private bool _canMove = false, _arrived = false;

    void Awake()
    {
        _targetPos = _positions[0].position;
        _secTM.onRunning += AvailableToMove;
        
        //_activator.onActivated += AvailableToMove;
    }

    void Update()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        if (!_canMove)
        {
            _targetPos = _positions[0].position;

            if (Vector3.Distance(transform.position, _targetPos) > 0.01f)
            {
                MoveToTarget();
            }
        }

        if (_canMove && !_arrived)
        {
            if (Vector3.Distance(transform.position, _targetPos) > 0.01f)
            {
                MoveToTarget();
            }
            else
            {
                _index++;

                if (_index == _positions.Count())
                {
                    _index = 0;
                }

                StartCoroutine(WaitToNextTarget());
                _targetPos = _positions[_index].position;
            }
        }
    }

    private void MoveToTarget()
    {
        Vector3 dir = _targetPos - transform.position;
        transform.position += dir.normalized * Time.deltaTime * _moveSpeed;
    }

    public void AvailableToMove(bool isActive)
    {
        _canMove = isActive;
    }

    private IEnumerator WaitToNextTarget()
    {
        _arrived = true;
        yield return new WaitForSeconds(1f);
        _arrived = false;
    }
}
