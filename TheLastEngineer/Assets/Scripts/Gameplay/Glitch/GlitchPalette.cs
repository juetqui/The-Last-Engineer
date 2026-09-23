using UnityEngine;

/// <summary>
/// Los tres colores del sistema de glitch, en un solo lugar. Antes estaban hardcodeados y
/// duplicados entre NodeController, OutlineController, CristalNodeView, BackPackColorChange y
/// UpdateCrosshair; con tres niveles en vez de dos esa duplicacion se vuelve inmanejable.
///
/// Se lee desde Resources (Default) y no por referencia serializada: asi ningun prefab ni escena
/// necesita cablearla, y no hay forma de que dos consumidores terminen apuntando a paletas distintas.
/// </summary>
[CreateAssetMenu]
public class GlitchPalette : ScriptableObject
{
    private const string ResourcePath = "GlitchPalette";

    [ColorUsage(true, true)]
    [SerializeField] private Color _clean = new Color(0f, 1f, 1f);          // #00FFFF

    [ColorUsage(true, true)]
    [SerializeField] private Color _intangible = new Color(1f, 0.839f, 0f); // #FFD600

    [ColorUsage(true, true)]
    [SerializeField] private Color _glitched = new Color(0.949f, 0f, 1f);   // #F200FF

    // Los emisivos (outline, cristal, mochila) necesitan HDR por encima de 1 para que el bloom los
    // levante; la UI y las particulas no. Un multiplicador comun evita tener seis colores que
    // mantener sincronizados a mano.
    [Tooltip("Multiplicador lineal que se aplica al color base para los materiales emisivos.")]
    [SerializeField] private float _emissionIntensity = 3f;

    private static GlitchPalette _default;

    public static GlitchPalette Default
    {
        get
        {
            if (_default != null) return _default;

            _default = Resources.Load<GlitchPalette>(ResourcePath);

            if (_default == null)
            {
                Debug.LogWarning($"[GlitchPalette] No se encontro Resources/{ResourcePath}.asset: se usan los colores por defecto.");
                _default = CreateInstance<GlitchPalette>();
            }

            return _default;
        }
    }

    public Color ColorFor(GlitchState state)
    {
        switch (state)
        {
            case GlitchState.Intangible: return _intangible;
            case GlitchState.Glitched: return _glitched;
            default: return _clean;
        }
    }

    public Color EmissionFor(GlitchState state)
    {
        var color = ColorFor(state) * _emissionIntensity;
        color.a = 1f;

        return color;
    }
}
