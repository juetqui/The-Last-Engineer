using UnityEngine;

/// <summary>
/// Los tres colores del sistema de glitch, en un solo lugar. Antes estaban hardcodeados y
/// duplicados entre NodeController, OutlineController, CristalNodeView, BackPackColorChange y
/// UpdateCrosshair; con tres niveles en vez de dos esa duplicacion se vuelve inmanejable.
/// </summary>
[CreateAssetMenu]
public class GlitchPalette : ScriptableObject
{
    [ColorUsage(true, true)]
    [SerializeField] private Color _clean = new Color(0f, 1f, 1f);          // #00FFFF

    [ColorUsage(true, true)]
    [SerializeField] private Color _intangible = new Color(1f, 0.839f, 0f); // #FFD600

    [ColorUsage(true, true)]
    [SerializeField] private Color _glitched = new Color(0.949f, 0f, 1f);   // #F200FF

    public Color ColorFor(GlitchState state)
    {
        switch (state)
        {
            case GlitchState.Intangible: return _intangible;
            case GlitchState.Glitched: return _glitched;
            default: return _clean;
        }
    }
}
