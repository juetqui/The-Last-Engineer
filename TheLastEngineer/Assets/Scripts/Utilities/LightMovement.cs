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

    private Light _light;
    private float _halfDuration;
    private float _timeOffset;

    void Start()
    {
        _light = GetComponent<Light>();

        // Cada luz arranca en un punto distinto del ciclo y con una duracion levemente distinta,
        // asi varias luces con este script no quedan coordinadas al iniciar la escena.
        float duration = Mathf.Max(0.01f, _cycleDuration + Random.Range(-_durationVariation, _durationVariation));
        _halfDuration = duration * 0.5f;
        _timeOffset = Random.Range(0f, duration);
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time + _timeOffset, _halfDuration) / _halfDuration;
        _light.range = Mathf.Lerp(_minRange, _maxRange, t);
    }
}
