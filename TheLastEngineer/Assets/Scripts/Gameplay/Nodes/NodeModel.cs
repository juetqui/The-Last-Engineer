using UnityEngine;

public class NodeModel
{
    private Transform _transform = default;
    private float _initialY = default;
    private Vector3 _initialGlobalPosition = Vector3.zero;

    public NodeModel(Transform transform)
    {
        _transform = transform;
        _initialY = transform.position.y;
        _initialGlobalPosition = transform.position;
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

    public void ResetPos(Vector3 resetPos) => _initialGlobalPosition = resetPos;
}
