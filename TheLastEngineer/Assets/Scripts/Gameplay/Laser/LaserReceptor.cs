using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Charges while at least one laser in its required glitch state hits it, and is an activation source
/// (doors, tube lights) while fully charged. Lasers in any other state are ignored except for the
/// negative feedback hook. Timings come from a LaserReceptorConfig preset.
/// </summary>
public class LaserReceptor : MonoBehaviour, ILaserReceptor, IActivationSource
{
    private const float Empty = 0f;
    private const float Full = 1f;

    private static readonly int GlowStepId = Shader.PropertyToID("_GlowStep");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private LaserReceptorConfig _config;

    [Tooltip("Glitch state an emitting laser must be in to charge this receptor.")]
    [GlitchStateOptions(GlitchState.Clean, GlitchState.Intangible)]
    [SerializeField] private GlitchState _requiredState = GlitchState.Clean;

    [SerializeField] private ParticleSystem _completeFillPartcle;
    [SerializeField] private List<ParticleSystem> _FinishFillParticle;
    [SerializeField] private List<ParticleSystem> _hitPS = new List<ParticleSystem>();

    // Public on purpose: the legacy LaserReceptorChecker reads and writes them directly.
    public bool _isCompleted = false;
    public float _currentLoad = 0;

    private Collider _collider = default;
    private Renderer _renderer = default;
    private Material _material = default;
    private AudioSource _audioSource = default;

    // LaserModel notifies in enter/exit pairs with the same state, so a plain count of the valid
    // beams currently hitting is enough to know when the first arrives and the last leaves.
    private int _validHitCount = 0;
    private bool _isCurrentlyLoading;
    private bool _isCurrentlyUnloading;
    private Coroutine _chargeRoutine;
    private Tween _glowTween;

    public bool IsActive => _isCompleted;
    public event Action<bool> OnActivationChanged;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
        _hitPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());

        if (_config == null)
            Debug.LogError($"[LaserReceptor] {name} has no LaserReceptorConfig: it will ignore lasers.", this);

        if (!LaserReceptorRules.IsValidRequiredState(_requiredState))
        {
            Debug.LogError($"[LaserReceptor] {name} cannot require {_requiredState}: using Clean.", this);
            _requiredState = LaserReceptorRules.SanitizeRequiredState(_requiredState);
        }

        _material = _renderer.material;
        _material.SetFloat(GlowStepId, Empty);
        _material.SetColor(EmissionColorId, GlitchPalette.Default.EmissionFor(_requiredState));
    }

    private void OnDestroy()
    {
        _glowTween.Stop();

        // renderer.material made a per-instance copy that Unity does not free with the object.
        if (_material != null) Destroy(_material);
    }

    public void LaserReceived(GlitchState emitterState)
    {
        if (_config == null) return;

        if (!LaserReceptorRules.AcceptsEmitter(_requiredState, emitterState))
        {
            OnInvalidLaser(emitterState);
            return;
        }

        _validHitCount++;

        if (_validHitCount == 1) StartCharging();
    }

    public void LaserNotReceived(GlitchState emitterState)
    {
        if (_config == null || !LaserReceptorRules.AcceptsEmitter(_requiredState, emitterState)) return;

        if (_validHitCount == 0)
        {
            Debug.LogWarning($"[LaserReceptor] {name} got a LaserNotReceived without its LaserReceived.", this);
            return;
        }

        _validHitCount--;

        if (_validHitCount == 0) StopCharging();
    }

    public void SetUnCompleted() => SetCompleted(false);

    public void TurnOffObject()
    {
        _collider.enabled = false;
    }

    // Hook for the negative feedback (its own particles come in a later task). LaserModel notifies
    // only when a beam enters, so this logs once per entering beam, never per frame.
    private void OnInvalidLaser(GlitchState emitterState)
    {
        Debug.LogWarning($"[LaserReceptor] {name} requires a {_requiredState} laser; ignored a {emitterState} one.", this);
    }

    private void StartCharging()
    {
        // Completed means full and not emptying (emptying clears it), so there is nothing to charge.
        if (_isCurrentlyLoading || _isCompleted) return;

        _isCurrentlyUnloading = false;
        _isCurrentlyLoading = true;
        RunChargeRoutine(LoadRoutine());
    }

    private void StopCharging()
    {
        _completeFillPartcle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        StopHitFeedback();

        if (_isCompleted && !_config.CanBeUnfilled) return;
        if (_isCurrentlyUnloading) return;

        _isCurrentlyLoading = false;
        _isCurrentlyUnloading = true;
        SetCompleted(false);
        RunChargeRoutine(UnloadRoutine());
    }

    // Only one of Load/Unload runs at a time: a beam leaving and re-entering within one frame
    // would otherwise leave the previous routine alive next to the new one.
    private void RunChargeRoutine(IEnumerator routine)
    {
        if (_chargeRoutine != null) StopCoroutine(_chargeRoutine);

        _chargeRoutine = StartCoroutine(routine);
    }

    private IEnumerator LoadRoutine()
    {
        _audioSource.Stop();
        _audioSource.Play();

        foreach (var ps in _hitPS)
            ps.Play();

        TweenGlow(Full);

        while (_currentLoad < Full)
        {
            if (!_isCurrentlyLoading) yield break;

            _currentLoad += Time.deltaTime / _config.FillTime;
            yield return null;
        }

        _currentLoad = Full;
        _isCurrentlyLoading = false;

        foreach (var ps in _FinishFillParticle)
            ps.Play();

        _completeFillPartcle.Play();
        StopHitFeedback();
        SetCompleted(true);
    }

    private IEnumerator UnloadRoutine()
    {
        TweenGlow(Empty);

        while (_currentLoad > Empty)
        {
            if (!_isCurrentlyUnloading) yield break;

            _currentLoad -= Time.deltaTime / _config.UnfillTime;
            yield return null;
        }

        _currentLoad = Empty;
        _isCurrentlyUnloading = false;
    }

    private void StopHitFeedback()
    {
        _audioSource.Stop();

        foreach (var ps in _hitPS)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void TweenGlow(float target)
    {
        _glowTween.Stop();
        _glowTween = Tween.MaterialProperty(_material, GlowStepId, target, _config.GlowDuration);
    }

    private void SetCompleted(bool completed)
    {
        if (_isCompleted == completed) return;

        _isCompleted = completed;
        OnActivationChanged?.Invoke(completed);
    }
}
