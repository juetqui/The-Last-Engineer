using PrimeTween;
using UnityEngine;

/// <summary>
/// Always-visible scanner effect of one platform station (a renderer whose material has <c>_Color</c>;
/// <c>_Alpha</c> is left as the material sets it). While the platform heads here, <c>_Color</c> fades to the
/// station's blink color, and back to the material's own color once it arrives, leaves or loses power.
/// Driven by the <see cref="RouteStationLight"/> of the station. Subclasses add more reactions
/// (see <see cref="StationFalloffScannerEffect"/>).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class StationScannerEffect : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [Header("Color (to the blink color while targeted, back on arrival)")]
    [SerializeField, Min(0f)] private float _colorDuration = 0.5f;
    [SerializeField] private Ease _colorEase = Ease.OutSine;

    private Color _originalColor;
    private Color _color;
    private Tween _colorTween;

    /// <summary>This renderer's own material instance (destroyed with the component).</summary>
    protected Material Material { get; private set; }

    protected virtual void Awake()
    {
        Material = GetComponent<Renderer>().material;
        _originalColor = Material.GetColor(ColorId);
        _color = _originalColor;
    }

    protected virtual void OnDestroy()
    {
        _colorTween.Stop();

        if (Material != null)
            Destroy(Material);
    }

    /// <summary>Takes the blink color while the platform heads here, the material's own color otherwise.</summary>
    /// <param name="state">The new state of the station.</param>
    /// <param name="blinkColor">The station's blink color.</param>
    public virtual void SetState(StationScanState state, Color blinkColor)
    {
        TweenColor(state == StationScanState.Targeted ? blinkColor : _originalColor);
    }

    private void TweenColor(Color target)
    {
        _colorTween.Stop();

        if (_colorDuration <= 0f)
        {
            SetColor(target);
            return;
        }

        _colorTween = Tween.Custom(this, _color, target, _colorDuration, (effect, value) => effect.SetColor(value), _colorEase);
    }

    private void SetColor(Color value)
    {
        _color = value;
        Material.SetColor(ColorId, value);
    }
}
