using UnityEngine;
using UnityEngine.Rendering;

public class NodeModel
{
    private Transform _transform = default;
    private float _minY = default, _maxY = default, _moveSpeed = default, _rotSpeed = default, _initialY = default;
    private Vector3 scaleVector = new Vector3(0.0125f, 0.0125f, 0.0125f);
    private Quaternion _initialGlobalRotation = default;

    private Vector3 _initialGlobalPosition;

    public NodeModel(Transform transform, float minY, float maxY, float moveSpeed, float rotSpeed)
    {
        _transform = transform;
        _initialY = transform.position.y;
        _minY = minY;
        _maxY = maxY;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
        
        _initialGlobalRotation = transform.rotation;
        _initialGlobalPosition = transform.position;
    }

    //public void MoveObject()
    //{
    //    if (CheckForRoom(out Transform roomParent))
    //    {
    //        Quaternion parentRotation = roomParent.rotation;
    //        _transform.rotation = _initialGlobalRotation;
    //    }

    //    Vector3 currentPosition = _transform.localPosition;

    //    float offset = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);
    //    float newY = _initialY + offset;

    //    _transform.localPosition = new Vector3(currentPosition.x, newY, currentPosition.z);
    //    _transform.Rotate(0, _rotSpeed * Time.deltaTime, 0);
    //}

    public void MoveObject()
    {
        float offset = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);
        float newGlobalY = _initialGlobalPosition.y + offset;

        _initialY += _rotSpeed * Time.deltaTime;
        _transform.rotation = Quaternion.Euler(0, _initialY, 0);
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
            if (nodeType == NodeType.Dash)
                newScale = Vector3.one * 2;

            if (CheckForRoom(out newParent))
            {
                _transform.SetParent(newParent);
                _transform.localPosition = newPos;
            }
            else
            {
                _transform.SetParent(null);
                _transform.position = newPos;
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
        if (Physics.Raycast(_transform.position, -_transform.up, out RaycastHit hit, 5f) && hit.transform.gameObject.GetComponentInParent<Room>() != null)
        {
            newParent = hit.transform.gameObject.GetComponentInParent<Room>().transform;
            Debug.Log(newParent.name);
            return true;
        }

        newParent = null;
        return false;
    }
}
