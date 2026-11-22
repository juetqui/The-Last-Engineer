using PrimeTween;
using UnityEngine;

/// <summary>
/// <see cref="StationScannerEffect"/> that also drives <c>_Falloff</c>: it ping-pongs 0 ↔ 1 while the station is
/// idle. Once the platform heads here it rises to 1 (finishing the cycle if it was rising, turning back right away
/// if it was falling) and stays there until the platform leaves again; without power it rests at a set value.
/// </summary>
public class StationFalloffScannerEffect : StationScannerEffect
{
    private static readonly int FalloffId = Shader.PropertyToID("_Falloff");
    private const float MinFalloff = 0f;
    private const float MaxFalloff = 1f;
    private const int InfiniteCycles = -1;
    private const int CyclesPerRoundTrip = 2;

    private enum FalloffMode
    {
        /// <summary>Ping-ponging (including the first leg down to the minimum).</summary>
        PingPong,
        /// <summary>Heading to 1 because the platform targets this station; it stays at 1 once there.</summary>
        RisingToMax,
        /// <summary>Going to, or resting at, a fixed value (arrive or unpowered).</summary>
        Fixed,
    }

    [Header("Ping-pong (the platform is not resting here)")]
    [Tooltip("Seconds to go from one end of the falloff to the other (half a ping-pong). Also the speed of the rise to 1 when targeted.")]
    [SerializeField, Min(0.01f)] private float _pingPongHalfCycle = 1f;
    [SerializeField] private Ease _pingPongEase = Ease.InOutSine;

    [Header("Arrive (the platform rests here without having been targeted, e.g. power came back)")]
    [SerializeField, Min(0f)] private float _arriveDuration = 0.5f;
    [SerializeField] private Ease _arriveEase = Ease.OutSine;

    [Header("Unpowered (the station's lights are off)")]
    [SerializeField, Range(0f, 1f)] private float _unpoweredFalloff;
    [SerializeField, Min(0f)] private float _unpoweredDuration = 0.5f;
    [SerializeField] private Ease _unpoweredEase = Ease.InOutSine;

    private float _falloff;
    private FalloffMode _mode = FalloffMode.Fixed;
    // True once the yoyo loop (0 → 1 → 0 ...) runs; false during the first leg down to the minimum.
    private bool _inLoop;
    private Tween _falloffTween;

    protected override void Awake()
    {
        base.Awake();

        // Platforms start unpowered; PoweredOn moves every station on from here.
        SetFalloff(_unpoweredFalloff);
    }

    protected override void OnDestroy()
    {
        _falloffTween.Stop();
        base.OnDestroy();
    }

    /// <summary>Applies the base color reaction, then moves <c>_Falloff</c> for the new state.</summary>
    public override void SetState(StationScanState state, Color blinkColor)
    {
        base.SetState(state, blinkColor);

        switch (state)
        {
            case StationScanState.Idle:
                StartPingPong();
                break;
            case StationScanState.Targeted:
                RiseToMax();
                break;
            case StationScanState.Resting:
                // Already rising (it was the target) or at 1: let it get there at its own pace.
                if (_mode == FalloffMode.RisingToMax || IsFixedAtMax()) break;
                TweenFalloff(MaxFalloff, _arriveDuration, _arriveEase);
                break;
            case StationScanState.Unpowered:
                TweenFalloff(_unpoweredFalloff, _unpoweredDuration, _unpoweredEase);
                break;
            default:
                Debug.LogWarning($"{nameof(StationFalloffScannerEffect)} on '{name}': unhandled state {state}.", this);
                break;
        }
    }

    private bool IsFixedAtMax() => _mode == FalloffMode.Fixed && !_falloffTween.isAlive && _falloff >= MaxFalloff;

    // Already ping-ponging: keep the running cycle instead of restarting it.
    private void StartPingPong()
    {
        if (_mode == FalloffMode.PingPong) return;

        _mode = FalloffMode.PingPong;
        _inLoop = false;
        _falloffTween.Stop();

        // First leg down to the minimum, as long as a half cycle scaled by how far it has to go.
        float firstLeg = _pingPongHalfCycle * Mathf.Abs(_falloff - MinFalloff);
        if (firstLeg <= 0f)
        {
            SetFalloff(MinFalloff);
            PingPongLoop();
            return;
        }

        _falloffTween = Tween.Custom(this, _falloff, MinFalloff, firstLeg, (effect, value) => effect.SetFalloff(value), _pingPongEase)
            .OnComplete(this, effect => effect.PingPongLoop());
    }

    private void PingPongLoop()
    {
        _inLoop = true;
        _falloffTween = Tween.Custom(this, MinFalloff, MaxFalloff, _pingPongHalfCycle, (effect, value) => effect.SetFalloff(value),
            _pingPongEase, InfiniteCycles, CycleMode.Yoyo);
    }

    private void RiseToMax()
    {
        if (_mode == FalloffMode.RisingToMax || IsFixedAtMax()) return;

        // In Yoyo from MinFalloff an even number of finished cycles means it is on its way up.
        bool loopRising = _mode == FalloffMode.PingPong && _inLoop && _falloffTween.isAlive
                          && _falloffTween.cyclesDone % CyclesPerRoundTrip == 0;

        _mode = FalloffMode.RisingToMax;

        if (loopRising)
        {
            // Finish this rise with the same pace and ease, and stop at 1.
            _falloffTween.SetRemainingCycles(stopAtEndValue: true);
            return;
        }

        // Falling (or resting at another value): turn back now, at the ping-pong speed.
        _falloffTween.Stop();

        float duration = _pingPongHalfCycle * Mathf.Abs(MaxFalloff - _falloff);
        if (duration <= 0f)
        {
            SetFalloff(MaxFalloff);
            return;
        }

        _falloffTween = Tween.Custom(this, _falloff, MaxFalloff, duration, (effect, value) => effect.SetFalloff(value), _pingPongEase);
    }

    private void TweenFalloff(float target, float duration, Ease ease)
    {
        _mode = FalloffMode.Fixed;
        _falloffTween.Stop();

        if (duration <= 0f)
        {
            SetFalloff(target);
            return;
        }

        _falloffTween = Tween.Custom(this, _falloff, target, duration, (effect, value) => effect.SetFalloff(value), ease);
    }

    private void SetFalloff(float value)
    {
        _falloff = value;
        Material.SetFloat(FalloffId, value);
    }
}
