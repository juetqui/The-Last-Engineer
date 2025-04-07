using UnityEngine;

public class CombinerView
{
    private Renderer _renderer = default;
    private Color _noColor = default, _onColor = default, _offColor = default;

    public CombinerView(Renderer renderer, Color noColor, Color onColor, Color offColor)
    {
        _renderer = renderer;
        _noColor = noColor;
        _onColor = onColor;
        _offColor = offColor;
    }

    public void OnStart()
    {
        _renderer.material.color = _noColor;
    }

    public void Enabled(bool enable)
    {
        if (enable)
        {
            _renderer.material.color = _onColor;
        }
        else OnStart();
    }
}
