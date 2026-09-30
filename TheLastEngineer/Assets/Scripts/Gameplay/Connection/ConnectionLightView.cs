using UnityEngine;

/// <summary>
/// Point Light de la Connection. Usa el color base de GlitchPalette (no el emisivo): una Light
/// ya tiene su propia intensidad y no necesita HDR en el color.
/// </summary>
public class ConnectionLightView : ConnectionPulseView
{
    [SerializeField] private Color _lightOff;

    private Light _light = default;

    protected override Color OffColor => _lightOff;
    protected override Color CurrentColor => _light.color;

    protected override void CacheComponents() => _light = GetComponent<Light>();
    protected override Color OnColorFor(GlitchState level) => GlitchPalette.Default.ColorFor(level);
    protected override void ApplyColor(Color color) => _light.color = color;
}
