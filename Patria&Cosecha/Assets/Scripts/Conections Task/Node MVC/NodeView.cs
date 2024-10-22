using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Material _material = default;
    private Collider _collider = default;

    public NodeView(Renderer renderer, Material material, Collider collider)
    {
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
}
