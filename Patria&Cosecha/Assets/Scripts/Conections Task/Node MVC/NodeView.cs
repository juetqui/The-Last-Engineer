using UnityEngine;

public class NodeView
{
    private Transform _transform = default;
    private Renderer _renderer = default;
    private Material _material = default;
    private Collider _collider = default;

    public NodeView(Transform transform, Renderer renderer, Material material, Collider collider)
    {
        _transform = transform;
        _renderer = renderer;
        _material = material;
        _collider = collider;
    }

    public void OnStart()
    {
        _renderer.material = _material;
    }

    public void EnableColl(bool onOff)
    {
        _collider.enabled = onOff;
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

    //private void OnUpdate()
    //{
        
    //}
}
