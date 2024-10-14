using UnityEngine;

public class ConnectionsView
{
    private Material _material;
    private Color _color;

    public Material NewMaterial { get { return _material; } }

    public ConnectionsView(Material material, Color color)
    {
        _material = material;
        _color = color;
    }

    public void OnStart()
    {
        _material.color = _color;
    }

    public void OnUpdate()
    {
        
    }
}
