using UnityEngine;

public class NodeModel
{
    private Transform _transform = default;
    private float _minY = default, _maxY = default, _moveSpeed = default, _rotSpeed = default, _initialY = default;
    private Vector3 _scaleVector = new Vector3(0.0125f, 0.0125f, 0.0125f);
    private Vector3 _initialGlobalPosition;

    public NodeModel(Transform transform, float minY, float maxY, float moveSpeed, float rotSpeed)
    {
        _transform = transform;
        _initialY = transform.position.y;
        _minY = minY;
        _maxY = maxY;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
        _initialGlobalPosition = transform.position;
    }

    public void MoveObject()
    {
        float offset = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);
        float newGlobalY = _initialGlobalPosition.y + offset;

        _initialY += _rotSpeed * Time.deltaTime;
        _transform.position = new Vector3(_initialGlobalPosition.x, newGlobalY, _initialGlobalPosition.z);
    }
    

    public void SetPos(Vector3 newPos, NodeType nodeType, Transform newParent = null, Vector3 newScale = default)
    {
        _initialGlobalPosition = newPos;

        if (newParent != null)
        {
            _transform.SetParent(newParent, false);
            _transform.localPosition = newPos;
            _transform.rotation = Quaternion.LookRotation(newParent.forward);
        }
        else
        {
            _transform.SetParent(null);
            _transform.position = newPos;
        }

        _initialY = _transform.position.y;

        if (newScale != default) _transform.localScale = newScale;
        else _transform.localScale = Vector3.one;
    }
}
