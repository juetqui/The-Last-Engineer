using UnityEngine;

public class CombinerView
{
    private Renderer _renderer = default;
    private Color _onColor = default, _offColor = default;

    public CombinerView(Renderer renderer, Color onColor, Color offColor)
    {
        _renderer = renderer;
        _onColor = onColor;
        _offColor = offColor;
    }

    public void OnStart()
    {
        _renderer.material.color = _offColor;
    }

    public void Enabled()
    {
        _renderer.material.color = _onColor;
    }
}