using UnityEngine;

public class NodeModel
{
    private Transform _transform = default, _feedbackPos = default;
    private float _minY = default, _maxY = default, _moveSpeed = default, _rotSpeed = default, _initialY = default;
    private Vector3 _scaleVector = new Vector3(0.0125f, 0.0125f, 0.0125f);
    private Vector3 _initialGlobalPosition = Vector3.zero;

    public NodeModel(Transform transform, Transform feedbackPos, float minY, float maxY, float moveSpeed, float rotSpeed)
    {
        _transform = transform;
        _feedbackPos = feedbackPos;
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
        float newGlobalY = _transform.position.y + offset;

        _initialY += _rotSpeed * Time.deltaTime;
        _transform.position = new Vector3(_transform.position.x, newGlobalY, _transform.position.z);
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

    public void ResetPos(Vector3 resetPos)
    {
        _transform.position = resetPos;
    }

    public void RotateToTarget(Transform target)
    {
        if (target == null) return;

        Vector3 directionToTarget = target.position - _feedbackPos.position;
        directionToTarget.y = 0;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            _feedbackPos.rotation = Quaternion.Slerp(_feedbackPos.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }
}
