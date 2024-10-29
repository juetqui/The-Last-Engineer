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
            Vector3 direction = (UpdateTargetPos() - transform.position).normalized;
            transform.position += direction * _speed * Time.deltaTime;
        }
        else if (_index < _movePoints.Length)
        {
            if (_goUp) _index++;
            else _index--;

            _currentTarget = _movePoints[_index];
        }
        else  if (_index >= _movePoints.Length) _goUp = false;
        else if (_index == 0) _goUp = true;
    }

    private Vector3 UpdateTargetPos()
    {
        return new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
    }
}
