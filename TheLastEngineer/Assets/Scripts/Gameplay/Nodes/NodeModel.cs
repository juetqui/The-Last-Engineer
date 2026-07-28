using UnityEngine;

public class NodeModel
{
    private Transform _transform = default;

    public NodeModel(Transform transform)
    {
        _transform = transform;
    }

    public void SetPos(Vector3 newPos, NodeType nodeType, Transform newParent = null, Vector3 newScale = default, Quaternion newRot = default)
    {
        if (newParent != null)
        {
            _transform.SetParent(newParent, false);
            _transform.localPosition = newPos;
            _transform.rotation = newRot;
        }
        else
        {
            _transform.SetParent(null);
            _transform.position = newPos;
            _transform.rotation = newRot;
        }

        if (newScale != default) _transform.localScale = newScale;
        else _transform.localScale = Vector3.one;
    }
}
