using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Material _material = default;
    private Collider _collider = default;
    private Outline _outline = default;
    private Color _outlineColor = default;

    public NodeView(Renderer renderer, Material material, Collider collider, Outline outline, Color outlineColor)
    {
        _renderer = renderer;
        _material = material;
        _collider = collider;
        _outline = outline;
        _outlineColor = outlineColor;
    }

    public void OnStart()
    {
        _renderer.material = _material;
        _outline.OutlineColor = _outlineColor;
    }

    public void EnableColl(bool onOff)
    {
        _collider.enabled = onOff;
    }
}
