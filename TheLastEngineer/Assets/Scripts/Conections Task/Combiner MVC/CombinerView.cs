using Unity.VisualScripting;
using UnityEngine;

public class CombinerView
{
    private Renderer _renderer = default, _shield = default;
    private Color _noColor = default, _onColor = default, _offColor = default;

    public CombinerView(Renderer renderer, Renderer shield, Color noColor, Color onColor, Color offColor)
    {
        _renderer = renderer;
        _shield = shield;
        _noColor = noColor;
        _onColor = onColor;
        _offColor = offColor;
    }

    public void OnStart()
    {
        _renderer.material.color = _noColor;
        _shield.material.SetColor("_Color", _noColor);
    }

    public void Enabled(bool enable)
    {
        if (enable)
        {
            _renderer.material.color = _onColor;
            _shield.material.SetColor("_Color", _onColor);
        }
        else OnStart();
    }
}
