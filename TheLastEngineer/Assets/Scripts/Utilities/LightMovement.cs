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

    private void OnValidate()
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

    void OnEnable()
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
        var rng = new System.Random(GetStableSeed());

        float variation = (float)(rng.NextDouble() * 2d - 1d) * _durationVariation;
        float duration = Mathf.Max(0.01f, _cycleDuration + variation);

        _halfDuration = duration * 0.5f;
        _timeOffset = (float)rng.NextDouble() * duration;

        RestartTween();
    }

    /// <summary>
    /// Un unico tween infinito en yoyo reemplaza al Update por frame. En CycleMode.Yoyo cada ciclo es
    /// medio recorrido (min -> max), por eso la duracion del tween es la mitad del ciclo completo.
    /// </summary>
    private void RestartTween()
    {
        // PrimeTween no corre en edit mode: ahi la oscilacion la dibuja la preview del LightMovementEditor.
        if (!Application.isPlaying || !isActiveAndEnabled) return;

        if (_light == null)
            _light = GetComponent<Light>();

        if (_light == null) return;

        if (!_hasOriginalRange)
        {
            _originalRange = _light.range;
            _hasOriginalRange = true;
        }

        _tween.Stop();
        _tween = Tween.LightRange(_light, _minRange, _maxRange, _halfDuration, _easeType,
            cycles: -1, cycleMode: CycleMode.Yoyo, useUnscaledTime: _useUnscaledTime);

        // Arranca cada luz en un punto distinto del ciclo (el offset puede caer en cualquiera de las dos mitades).
        _tween.elapsedTimeTotal = _timeOffset;
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
