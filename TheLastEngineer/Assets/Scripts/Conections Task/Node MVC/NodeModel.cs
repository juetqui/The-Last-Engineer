using UnityEditor.Search;
using UnityEngine;

public class NodeModel
{
    private Transform _transform = default;
    private float _minY = default, _maxY = default, _moveSpeed = default, _rotSpeed = default, _initialY = default;
    private Vector3 scaleVector = new Vector3(0.0125f, 0.0125f, 0.0125f);

    public NodeModel(Transform transform, float minY, float maxY, float moveSpeed, float rotSpeed)
    {
        _transform = transform;
        _initialY = transform.position.y;
        _minY = minY;
        _maxY = maxY;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
    }

    public void MoveObject()
    {
        Vector3 currentPosition = _transform.position;

        float offset = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);
        float newY = _initialY + offset;

        _transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        _transform.Rotate(0, _rotSpeed * Time.deltaTime, 0);
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

            if (CheckForRoom(out newParent))
            {
                _transform.SetParent(newParent);
                _transform.position = newPos;
            }
            else
            {
                _transform.SetParent(null);
                _transform.localPosition = newPos;
            }
        }

        _initialY = _transform.position.y;

        if (newScale != default) _transform.localScale = newScale;
        else _transform.localScale = Vector3.one;
    }

    public bool Combine(float deltaTime)
    {
        bool result = false;

        if (_transform.localScale.magnitude > scaleVector.magnitude)
        {
            _transform.localScale += -(Vector3.one * 2f * deltaTime);
            result = false;
        }
        else result = true;

        return result;
    }

    public bool CheckForRoom(out Transform newParent)
    {
        RaycastHit hit;

        if (Physics.Raycast(_transform.position, -_transform.up, out hit, 5f) && hit.transform.gameObject.GetComponentInParent<Room>() != null)
        {
            newParent = hit.transform.gameObject.GetComponentInParent<Room>().transform;
            return true;
        }

        newParent = null;
        return false;
    }
}
