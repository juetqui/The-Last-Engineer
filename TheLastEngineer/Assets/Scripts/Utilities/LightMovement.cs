using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightMovement : MonoBehaviour
{
    [Header("Range Oscillation")]
    [SerializeField] private float _minRange = 8f;
    [SerializeField] private float _maxRange = 15f;
    [Tooltip("Segundos que tarda un ciclo completo (min -> max -> min)")]
    [SerializeField] private float _cycleDuration = 1f;
    [Tooltip("Variacion aleatoria de la duracion por luz, en segundos, para desincronizarlas")]
    [SerializeField] private float _durationVariation = 0.1f;
    [SerializeField] private Ease _easeType = Ease.InOutSine;
    [Tooltip("Sigue oscilando aunque el juego este pausado (timeScale = 0)")]
    [SerializeField] private bool _useUnscaledTime = false;

    private Light _light;
    private Tween _tween;
    private float _halfDuration;
    private float _timeOffset;
    private float _originalRange;
    private bool _hasOriginalRange;

    public float MinRange => _minRange;
    public float MaxRange => _maxRange;
    public Ease EaseType => _easeType;

    private const int InfiniteCycles = -1;

    /// <summary>Subclasses return false to give every instance the same cycle (no offset, no duration variation).</summary>
    protected virtual bool UseDesync => true;

    /// <summary>Subclasses return false to keep the light still at its min range instead of oscillating.</summary>
    protected virtual bool ShouldAnimate => true;

    protected virtual void OnValidate()
    {
        _maxRange = Mathf.Max(_minRange, _maxRange);
        _cycleDuration = Mathf.Max(0.01f, _cycleDuration);
        _durationVariation = Mathf.Max(0f, _durationVariation);

        // Asi un cambio de duracion se refleja al toque mientras se previsualiza desde el inspector.
        InitializeCycle();
    }

    void Awake()
    {
        _light = GetComponent<Light>();
    }

    protected virtual void OnEnable()
    {
        InitializeCycle();
    }

    void OnDisable()
    {
        _tween.Stop();
        RestoreOriginalRange();
    }

    /// <summary>
    /// Cada luz arranca en un punto distinto del ciclo y con una duracion levemente distinta, asi varias
    /// luces con este script no quedan coordinadas. La semilla se deriva del objeto y no de Random, para
    /// que el desfase sea el mismo en el editor y en play.
    /// </summary>
    public void InitializeCycle()
    {
        float duration = _cycleDuration;
        _timeOffset = 0f;

        if (UseDesync)
        {
            var rng = new System.Random(GetStableSeed());

            float variation = (float)(rng.NextDouble() * 2d - 1d) * _durationVariation;
            duration = Mathf.Max(0.01f, _cycleDuration + variation);
            _timeOffset = (float)rng.NextDouble() * duration;
        }

        _halfDuration = duration * 0.5f;

        RestartTween();
    }

    /// <summary>
    /// Oscillates again. A cycle still finishing towards min (<see cref="FinishCycleAtMin"/>) just goes back
    /// to looping from where it is, without a jump. A light already resting at min restarts at its cycle
    /// offset, so with desync on it can jump from min to that point.
    /// </summary>
    protected void ResumeOscillation()
    {
        if (_tween.isAlive)
            _tween.SetRemainingCycles(InfiniteCycles);
        else
            RestartTween();
    }

    /// <summary>
    /// Lets the current cycle run until the range reaches min and stops there. With no oscillation running,
    /// the range is set to min right away.
    /// </summary>
    protected void FinishCycleAtMin()
    {
        if (!CanDriveLight()) return;

        if (_tween.isAlive)
        {
            // In Yoyo the tween's start value is _minRange: stop the next time it gets there.
            _tween.SetRemainingCycles(stopAtEndValue: false);
            return;
        }

        CaptureOriginalRange();
        _light.range = _minRange;
    }

    /// <summary>
    /// Un unico tween infinito en yoyo reemplaza al Update por frame. En CycleMode.Yoyo cada ciclo es
    /// medio recorrido (min -> max), por eso la duracion del tween es la mitad del ciclo completo.
    /// </summary>
    private void RestartTween()
    {
        if (!ShouldAnimate || !CanDriveLight()) return;

        CaptureOriginalRange();

        _tween.Stop();
        _tween = Tween.LightRange(_light, _minRange, _maxRange, _halfDuration, _easeType,
            cycles: -1, cycleMode: CycleMode.Yoyo, useUnscaledTime: _useUnscaledTime);

        // Arranca cada luz en un punto distinto del ciclo (el offset puede caer en cualquiera de las dos mitades).
        _tween.elapsedTimeTotal = _timeOffset;
    }

    // PrimeTween no corre en edit mode: ahi la oscilacion la dibuja la preview del LightMovementEditor.
    private bool CanDriveLight()
    {
        if (!Application.isPlaying || !isActiveAndEnabled) return false;

        if (_light == null)
            _light = GetComponent<Light>();

        return _light != null;
    }

    private void CaptureOriginalRange()
    {
        if (_hasOriginalRange) return;

        _originalRange = _light.range;
        _hasOriginalRange = true;
    }

    private void RestoreOriginalRange()
    {
        if (!_hasOriginalRange || _light == null) return;

        _light.range = _originalRange;
        _hasOriginalRange = false;
    }

    /// <summary>Valor de range que le corresponde a esta luz en el tiempo indicado. Solo para la preview del editor.</summary>
    public float EvaluateRange(float time)
    {
        if (_halfDuration <= 0f)
            InitializeCycle();

        float t = Mathf.PingPong(time + _timeOffset, _halfDuration) / _halfDuration;
        return Mathf.Lerp(_minRange, _maxRange, Easing.Evaluate(t, PreviewEase));
    }

    /// <summary>Aplica el range del ciclo a la Light. Lo usa la preview del editor, fuera de play mode.</summary>
    public void ApplyRange(float time)
    {
        if (_light == null)
            _light = GetComponent<Light>();

        if (_light != null)
            _light.range = EvaluateRange(time);
    }

    // Ease.Custom loguea error en Easing.Evaluate y Ease.Default instancia el PrimeTweenManager de forma
    // lazy (spawnearia un GameObject durante la preview en edit mode), asi que ninguno llega hasta ahi.
    private Ease PreviewEase =>
        _easeType == Ease.Custom || _easeType == Ease.Default ? Ease.InOutSine : _easeType;

    // FNV-1a sobre el nombre + la posicion serializada: valores identicos en edit mode y en play mode
    // (a diferencia de GetInstanceID o de string.GetHashCode).
    private int GetStableSeed()
    {
        uint hash = 2166136261u;
        string objectName = name;

        for (int i = 0; i < objectName.Length; i++)
            hash = (hash ^ objectName[i]) * 16777619u;

        hash ^= (uint)transform.localPosition.GetHashCode();
        return (int)hash;
    }
}
