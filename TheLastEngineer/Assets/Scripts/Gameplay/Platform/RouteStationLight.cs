using PrimeTween;
using UnityEngine;

/// <summary>
/// Lights of one platform station: the emissive of the renderers on its direct children, and the color of the
/// Light under each of those renderers. Goes on the route
/// point (a station of a <see cref="PlatformController"/>). Blinks while a platform heads here, stays on
/// once it arrives, and is off while the platform is unpowered. Also tells every <see cref="StationScannerEffect"/>
/// among its children the station's <see cref="StationScanState"/>. Driven by <see cref="PlatformStationLights"/>.
/// </summary>
public class RouteStationLight : MonoBehaviour
{
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private const int ExpectedLights = 4;
    private const int InfiniteCycles = -1;

    [Header("Blink (a platform is heading here)")]
    [ColorUsage(true, true)]
    [SerializeField] private Color _blinkColor = Color.cyan;
    [Tooltip("Seconds to go from the blink color to black (half a blink).")]
    [SerializeField, Min(0.01f)] private float _blinkHalfCycle = 0.5f;
    [SerializeField] private Ease _blinkEase = Ease.InOutSine;

    [Header("On / off (platform arrived / powered off)")]
    [ColorUsage(true, true)]
    [SerializeField] private Color _onColor = Color.white;
    [SerializeField, Min(0f)] private float _switchDuration = 0.5f;
    [SerializeField] private Ease _switchEase = Ease.OutSine;

    // Every scanner among the children, told each state change of this station.
    private StationScannerEffect[] _scanners = System.Array.Empty<StationScannerEffect>();
    private Material[] _materials = System.Array.Empty<Material>();
    private Light[] _lights = System.Array.Empty<Light>();
    private StationLightMovement[] _movements = System.Array.Empty<StationLightMovement>();
    private Color _current = Color.black;
    private Tween _tween;

    private void Awake()
    {
        _scanners = GetComponentsInChildren<StationScannerEffect>(true);

        int count = 0;
        int lightCount = 0;
        int movementCount = 0;
        var materials = new Material[transform.childCount];
        var lights = new Light[transform.childCount];
        var movements = new StationLightMovement[transform.childCount];

        foreach (Transform child in transform)
        {
            // Scanners have their own renderer but are not lights.
            if (child.TryGetComponent(out StationScannerEffect _)) continue;
            if (!child.TryGetComponent(out Renderer childRenderer)) continue;

            materials[count++] = childRenderer.material;

            // Each light mesh carries a Light below it that glows with the same color.
            Light childLight = child.GetComponentInChildren<Light>(true);
            if (childLight == null)
            {
                Debug.LogWarning($"{nameof(RouteStationLight)} on '{name}': renderer '{child.name}' has no child Light.", this);
                continue;
            }

            lights[lightCount++] = childLight;

            if (childLight.TryGetComponent(out StationLightMovement movement))
                movements[movementCount++] = movement;
            else if (childLight.TryGetComponent(out LightMovement _))
                Debug.LogWarning($"{nameof(RouteStationLight)} on '{name}': light '{childLight.name}' uses {nameof(LightMovement)}; swap it for {nameof(StationLightMovement)} so it can be switched on and off.", this);
        }

        System.Array.Resize(ref materials, count);
        System.Array.Resize(ref lights, lightCount);
        System.Array.Resize(ref movements, movementCount);
        _materials = materials;
        _lights = lights;
        _movements = movements;

        // The range oscillation only runs while a platform heads here (see StartBlinking).
        SetMoving(false);

        if (count != ExpectedLights)
            Debug.LogWarning($"{nameof(RouteStationLight)} on '{name}' found {count} child renderers, expected {ExpectedLights}.", this);

        Apply(Color.black);
    }

    private void OnDestroy()
    {
        _tween.Stop();

        foreach (Material material in _materials)
        {
            if (material != null)
                Destroy(material);
        }
    }

    /// <summary>Powered, not the target: fades to the on color and stays lit. Scanners go idle.</summary>
    public void TurnOn()
    {
        SetScannerState(StationScanState.Idle);
        FadeTo(_onColor);
    }

    /// <summary>Fades to black. Scanners go unpowered.</summary>
    public void TurnOff()
    {
        SetScannerState(StationScanState.Unpowered);
        FadeTo(Color.black);
    }

    /// <summary>The platform arrived here (or rests here when powered on): lights on, scanners resting.</summary>
    public void Arrived()
    {
        SetScannerState(StationScanState.Resting);
        FadeTo(_onColor);
    }

    /// <summary>The platform left this station: scanners go idle (the lights stay as they are).</summary>
    public void Departed() => SetScannerState(StationScanState.Idle);

    /// <summary>Fades to the blink color, then pulses blink color ↔ black until told otherwise. Scanners targeted.</summary>
    public void StartBlinking()
    {
        SetScannerState(StationScanState.Targeted);
        SetMoving(true);
        _tween.Stop();
        _tween = Tween.Custom(this, _current, _blinkColor, _blinkHalfCycle, (light, c) => light.Apply(c), _blinkEase)
            .OnComplete(this, light => light.BlinkLoop());
    }

    private void BlinkLoop()
    {
        _tween = Tween.Custom(this, _blinkColor, Color.black, _blinkHalfCycle, (light, c) => light.Apply(c),
            _blinkEase, InfiniteCycles, CycleMode.Yoyo);
    }

    private void FadeTo(Color target)
    {
        // Turning on or off means this station stopped being the platform's target.
        SetMoving(false);
        _tween.Stop();

        if (_switchDuration <= 0f)
        {
            Apply(target);
            return;
        }

        _tween = Tween.Custom(this, _current, target, _switchDuration, (light, c) => light.Apply(c), _switchEase);
    }

    private void SetScannerState(StationScanState state)
    {
        foreach (StationScannerEffect scanner in _scanners)
        {
            if (scanner != null)
                scanner.SetState(state, _blinkColor);
        }
    }

    // Stopping lets each light finish its current cycle and rest at its min range.
    private void SetMoving(bool moving)
    {
        foreach (StationLightMovement movement in _movements)
        {
            if (movement != null)
                movement.Animate = moving;
        }
    }

    private void Apply(Color color)
    {
        _current = color;

        foreach (Material material in _materials)
            material.SetColor(EmissiveColorId, color);

        foreach (Light light in _lights)
        {
            if (light != null)
                light.color = color;
        }
    }
}
