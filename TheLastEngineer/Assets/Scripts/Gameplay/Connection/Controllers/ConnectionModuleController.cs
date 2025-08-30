using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ConnectionModuleController : MonoBehaviour
{
    [SerializeField] private TaskManager _tm;
    [SerializeField] private ParticleSystem _orbitPs;
    [SerializeField] private ParticleSystem _completedPs;
    [SerializeField] private SplineAnimate _animator;
    [SerializeField] private Light _light;
    [SerializeField] private float _turnTime = 1f;
    [SerializeField] private float _maxLight = 5f;

    private Coroutine _lightRoutine;

    void Awake()
    {
        if (_tm == null) _tm = FindObjectOfType<TaskManager>();
        if (_tm != null) _tm.RunningChanged += SetFX;
    }

    void Start() => StopFX();

    void OnDestroy()
    {
        if (_tm != null) _tm.RunningChanged -= SetFX;
    }

    private void SetFX(bool running)
    {
        if (running)
            PlayFX();
        else
            StopFX();
    }


    private void StopFX()
    {
        _orbitPs.SafeStop();
        _completedPs.SafeStop();
        _animator?.Pause();
        StartLightRoutine(0f);
    }

    private void PlayFX()
    {
        _orbitPs.SafePlay();
        _completedPs.SafePlay();
        _animator?.Play();
        StartLightRoutine(_maxLight);
    }

    private void StartLightRoutine(float target)
    {
        if (_light == null) return;
        if (_lightRoutine != null) StopCoroutine(_lightRoutine);
        _lightRoutine = StartCoroutine(LerpLight(_light.intensity, target, _turnTime));
    }

    private IEnumerator LerpLight(float from, float to, float duration)
    {
        if (duration <= 0f) { _light.intensity = to; yield break; }
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _light.intensity = Mathf.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
    }
}
