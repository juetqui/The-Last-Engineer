using UnityEngine;

public class NodeModel
{
    private Transform _transform = default;
    private float _minY = default, _maxY = default, _moveSpeed = default, _rotSpeed = default;
    private Vector3 scaleVector = new Vector3(0.0125f, 0.0125f, 0.0125f);

    public NodeModel(Transform transform, float minY, float maxY, float moveSpeed, float rotSpeed)
    {
        _transform = transform;
        _minY = minY;
        _maxY = maxY;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
    }

    public void MoveObject(float deltaTime)
    {
        Vector3 currentPosition = _transform.position;
        float newY = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);

        _transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        _transform.Rotate(0, _rotSpeed * deltaTime, 0);
    }

    public void SetPos(Vector3 newPos, NodeType nodeType, Transform newParent = null, Vector3 newScale = default)
    {
        if (newParent != null)
        {
            _transform.SetParent(newParent, false);
            _transform.localPosition = newPos;
            _transform.rotation = Quaternion.LookRotation(newParent.forward);
        }
        else
        {
            if (nodeType == NodeType.Dash)
                newScale = Vector3.one * 2;

            _transform.SetParent(null);
            _transform.localPosition = newPos;
        }

        if (newScale != default) _transform.localScale = newScale;
        else _transform.localScale = Vector3.one;
    }

    public bool Combine(float deltaTime)
    {
        bool result = false;

        if (_transform.localScale.magnitude > scaleVector.magnitude)
        {
            _transform.localScale -= Vector3.one * 2f * deltaTime;
            result = false;
        }
        else result = true;

        return result;
    }
}
