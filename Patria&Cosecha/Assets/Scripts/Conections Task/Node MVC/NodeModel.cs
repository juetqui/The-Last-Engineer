using UnityEngine;

public class NodeModel
{
    private Transform _transform = default;
    private Vector3 scaleVector = new Vector3(0.25f, 0.25f, 0.25f);

    public NodeModel(Transform transform)
    {
        _transform = transform;
    }

    void Update()
    {
        
    }

    public void SetPos(Vector3 newPos, Transform newParent = null, Vector3 newScale = default)
    {
        if (newParent != null)
        {
            _transform.SetParent(newParent, false);
            _transform.localPosition = newPos;
            _transform.rotation = Quaternion.LookRotation(newParent.forward);
        }
        else
        {
            _transform.SetParent(null);
            _transform.localPosition = newPos;
        }

        if (newScale != default) _transform.localScale = newScale;
        else _transform.localScale = Vector3.one;
    }

    public void Combine(float deltaTime)
    {
        while (_transform.localScale.magnitude > scaleVector.magnitude) _transform.localScale -= Vector3.one * deltaTime;
    }
}
