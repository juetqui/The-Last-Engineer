using PrimeTween;
using UnityEngine;

/// <summary>
/// Plays the platform's hum loop while it is powered: fades in when a connection powers it on and fades out
/// once it comes to rest unpowered (Inactive), then stops the source. From the moment the player gets on
/// until they get off, the hum drops in pitch and gets low-passed so it sounds loaded. Decisions live in
/// <see cref="PlatformHumRules"/>; this component only wires them to the AudioSource, the filter and PrimeTween.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class PlatformAudio : MonoBehaviour
{
    private const int MinPriority = 0;
    private const int MaxPriority = 256;
    private const float MinPitch = 0.5f;
    private const float MaxPitch = 1.5f;
    private const float MinCutoff = 10f;
    private const float MaxCutoff = 22000f;
    private const float EmptyLoad = 0f;
    private const float FullLoad = 1f;

    [SerializeField] private PlatformController _platform;
    [SerializeField] private AudioClip _loopClip;

    [Header("Volume & fades")]
    [SerializeField, Range(0f, 1f)] private float _volume = 0.6f;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.8f;
    [SerializeField] private Ease _fadeInEase = Ease.OutSine;
    [SerializeField, Min(0f)] private float _fadeOutDuration = 1.2f;
    [SerializeField] private Ease _fadeOutEase = Ease.InSine;

    [Header("3D")]
    [SerializeField, Range(0f, 1f)] private float _spatialBlend = 1f;
    [SerializeField, Min(0f)] private float _minDistance = 2f;
    [SerializeField, Min(0f)] private float _maxDistance = 20f;
    [SerializeField] private AudioRolloffMode _rolloff = AudioRolloffMode.Logarithmic;
    [SerializeField, Range(0f, 5f)] private float _dopplerLevel = 0f;

    [Tooltip("Higher number = lower priority. Above the default 128 so one-shot SFX win voices over many hums.")]
    [SerializeField, Range(MinPriority, MaxPriority)] private int _priority = 160;

    [Header("Passenger load (player on the platform)")]
    [SerializeField, Range(MinPitch, MaxPitch)] private float _emptyPitch = 1f;
    [SerializeField, Range(MinPitch, MaxPitch)] private float _loadedPitch = 0.92f;
    [SerializeField, Range(MinCutoff, MaxCutoff)] private float _emptyCutoff = MaxCutoff;
    [SerializeField, Range(MinCutoff, MaxCutoff)] private float _loadedCutoff = 3000f;
    [SerializeField, Range(1f, 10f)] private float _lowPassResonanceQ = 1f;
    [SerializeField, Min(0f)] private float _loadInDuration = 0.35f;
    [SerializeField] private Ease _loadInEase = Ease.OutSine;
    [SerializeField, Min(0f)] private float _loadOutDuration = 0.6f;
    [SerializeField] private Ease _loadOutEase = Ease.InOutSine;

    [Header("Landing one-shot (optional)")]
    [Tooltip("Played through SFXManager when the player lands on a moving platform. Empty = no landing sound.")]
    [SerializeField] private AudioClip _landingClip;
    [SerializeField, Range(0f, 1f)] private float _landingVolume = 0.8f;

    private AudioSource _source;
    private AudioLowPassFilter _lowPass;
    private HumState _state = HumState.Silent;
    private Tween _fade;
    private Tween _loadTween;
    private float _load = EmptyLoad;
    private bool _hasPassenger;
    private bool _isMoving;
    private bool _subscribed;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _lowPass = GetComponent<AudioLowPassFilter>();

        if (_platform == null)
            _platform = GetComponent<PlatformController>();

        if (_platform == null || _loopClip == null)
        {
            Debug.LogError($"{nameof(PlatformAudio)} on '{name}' needs a {nameof(PlatformController)} and a loop clip; disabling it.", this);
            enabled = false;
            return;
        }

        _source.clip = _loopClip;
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = 0f;
        _source.spatialBlend = _spatialBlend;
        _source.minDistance = _minDistance;
        _source.maxDistance = _maxDistance;
        _source.rolloffMode = _rolloff;
        _source.dopplerLevel = _dopplerLevel;
        _source.priority = _priority;

        _lowPass.lowpassResonanceQ = _lowPassResonanceQ;
        ApplyLoad(EmptyLoad);
    }

    // Subscribed in OnEnable, which runs before Connection.Start announces connections that start plugged.
    private void OnEnable()
    {
        if (_platform == null || _subscribed) return;

        _platform.PoweredOn += HandlePoweredOn;
        _platform.BecameInactive += HandleBecameInactive;
        _platform.PassengerChanged += HandlePassengerChanged;
        _platform.MovingChanged += HandleMovingChanged;
        _subscribed = true;
    }

    private void OnDisable()
    {
        if (_subscribed && _platform != null)
        {
            _platform.PoweredOn -= HandlePoweredOn;
            _platform.BecameInactive -= HandleBecameInactive;
            _platform.PassengerChanged -= HandlePassengerChanged;
            _platform.MovingChanged -= HandleMovingChanged;
        }
        _subscribed = false;

        Silence();
    }

    private void OnDestroy()
    {
        _fade.Stop();
        _loadTween.Stop();
    }

    private void HandlePoweredOn() => Apply(HumSignal.PoweredOn);
    private void HandleBecameInactive() => Apply(HumSignal.BecameInactive);
    private void HandleFadeCompleted() => Apply(HumSignal.FadeCompleted);

    private void HandlePassengerChanged(bool hasPassenger)
    {
        // The platform can release its rider from its own OnDestroy, after this component was torn down.
        if (!isActiveAndEnabled) return;

        bool boarded = hasPassenger && !_hasPassenger;
        _hasPassenger = hasPassenger;

        if (boarded && _isMoving)
            PlayLanding();

        RefreshLoad();
    }

    private void HandleMovingChanged(bool isMoving)
    {
        if (!isActiveAndEnabled) return;

        // Only gates the landing one-shot: the loaded hum depends on the rider alone.
        _isMoving = isMoving;
    }

    private void Apply(HumSignal signal)
    {
        _state = PlatformHumRules.Next(_state, signal, out HumCommand command);

        switch (command)
        {
            case HumCommand.PlayAndFadeIn:
                _source.volume = 0f;
                _source.Play();
                StartFade(_volume, _fadeInDuration, _fadeInEase);
                break;
            case HumCommand.FadeIn:
                StartFade(_volume, _fadeInDuration, _fadeInEase);
                break;
            case HumCommand.FadeOut:
                StartFade(0f, _fadeOutDuration, _fadeOutEase);
                break;
            case HumCommand.Stop:
                _source.Stop();
                _source.volume = 0f;
                break;
            case HumCommand.None:
                break;
        }
    }

    private void StartFade(float target, float fullDuration, Ease ease)
    {
        _fade.Stop();

        float duration = PlatformHumRules.ScaledDuration(_source.volume, target, _volume, fullDuration);
        // Target overload: the lambda captures nothing, so it is cached and doesn't allocate per fade.
        _fade = Tween.AudioVolume(_source, target, duration, ease)
            .OnComplete(this, audio => audio.HandleFadeCompleted());
    }

    private void RefreshLoad()
    {
        // Loaded from the moment the player gets on until they get off, moving or not.
        bool loaded = _hasPassenger;
        float target = loaded ? FullLoad : EmptyLoad;

        if (Mathf.Approximately(_load, target) && !_loadTween.isAlive) return;

        _loadTween.Stop();

        float fullDuration = loaded ? _loadInDuration : _loadOutDuration;
        float duration = PlatformHumRules.ScaledDuration(_load, target, FullLoad, fullDuration);
        _loadTween = Tween.Custom(this, _load, target, duration,
            (audio, value) => audio.ApplyLoad(value), loaded ? _loadInEase : _loadOutEase);
    }

    private void ApplyLoad(float load)
    {
        _load = load;
        _source.pitch = PlatformHumRules.LoadedPitch(_emptyPitch, _loadedPitch, load);
        _lowPass.cutoffFrequency = PlatformHumRules.LoadedCutoff(_emptyCutoff, _loadedCutoff, load);
    }

    private void PlayLanding()
    {
        // Optional by design: no landing clip assigned yet means no landing sound.
        if (_landingClip == null) return;

        if (SFXManager.Instance == null)
        {
            Debug.LogWarning($"{nameof(PlatformAudio)} on '{name}': no {nameof(SFXManager)} in the scene, landing sound skipped.", this);
            return;
        }

        SFXManager.Instance.PlaySFX(_landingClip, transform, _landingVolume, true, _maxDistance);
    }

    private void Silence()
    {
        _fade.Stop();
        _loadTween.Stop();
        _state = HumState.Silent;
        _hasPassenger = false;
        _isMoving = false;

        if (_source == null) return;

        _source.Stop();
        _source.volume = 0f;

        if (_lowPass != null)
            ApplyLoad(EmptyLoad);
    }
}
