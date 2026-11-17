using UnityEngine;

/// <summary>
/// <see cref="LightMovement"/> for the lights of a platform station. The oscillation can be switched on and off
/// at runtime (driven by <see cref="RouteStationLight"/>): switching it off lets the current cycle finish and
/// leaves the light resting at its min range. The lights of one station can share their cycle or be desynced.
/// </summary>
public class StationLightMovement : LightMovement
{
    [Header("Station")]
    [Tooltip("On: each light gets its own offset and duration variation. Off: every light shares the same cycle.")]
    [SerializeField] private bool _desync = true;
    [Tooltip("On: the range oscillates. Off: the current cycle finishes at the min range and the light stays there.")]
    [SerializeField] private bool _animate;

    protected override bool UseDesync => _desync;
    protected override bool ShouldAnimate => _animate;

    public bool Animate
    {
        get => _animate;
        set
        {
            _animate = value;
            ApplyAnimate();
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        // The base already restarted the oscillation if it should animate; only the stop is left to apply.
        if (!_animate)
            FinishCycleAtMin();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (!_animate)
            FinishCycleAtMin();
    }

    private void ApplyAnimate()
    {
        if (_animate)
            ResumeOscillation();
        else
            FinishCycleAtMin();
    }
}
