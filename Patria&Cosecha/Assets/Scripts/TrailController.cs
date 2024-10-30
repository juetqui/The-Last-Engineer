using UnityEngine;

public class TrailController : MonoBehaviour
{
    [SerializeField] private Transform[] _movePoints;
    [SerializeField] private float _speed, _minDist;

    private Transform _currentTarget = default;
    private int _index = 0;
    private bool _goUp = true;

    void Start()
    {
        _currentTarget = _movePoints[_index];
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Vector3.Distance(transform.position, UpdateTargetPos()) > _minDist)
        {
            Vector3 dir = (UpdateTargetPos() - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;
        }
        else
        {
            if (_goUp) _index++;
            else _index--;

            if (_index >= _movePoints.Length)
            {
                _index--;
                _goUp = false;
                return;
            }
            else if (_index <= 0)
            {
                _index = 0;
                _goUp = true;
            }
            else _goUp = true;

            _currentTarget = _movePoints[_index];
        }
    }

    private Vector3 UpdateTargetPos()
    {
        return new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
    }
}
