using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Material _material = default;

    public NodeView(Renderer renderer, Material material)
    {
        _renderer = renderer;
        _material = material;
    }

    public void OnStart()
    {
        _renderer.material = _material;
    }

    //private void OnUpdate()
    //{
        
    //}
}
