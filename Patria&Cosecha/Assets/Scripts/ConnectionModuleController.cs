using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ConnectionModuleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _orbitPs;
    [SerializeField] private ParticleSystem _completedPs;
    [SerializeField] private SplineAnimate _animator;

    [Header("Light Management")]
    [SerializeField] private Light _light;
    [SerializeField] private float _turnTime;
    [SerializeField] private float _maxLight;

    private MainTM _mainTM = default;

    private void Start()
    {
        StopFX();
    }

    private void Update()
    {
        if (_mainTM.Running) PlayFX();
        else StopFX();
    }

    private void StopFX()
    {
        _orbitPs.Stop();
        _completedPs.Stop();
        DisableLight();
    }

    private void PlayFX()
    {
        if (!_orbitPs.isPlaying) _orbitPs.Play();
        if (!_completedPs.isPlaying) _completedPs.Play();
        EnableLight();
        _animator.Play();
    }

    private void EnableLight()
    {
        if (_light.intensity < _maxLight)
            _light.intensity += _turnTime * Time.deltaTime;
        else
            _light.intensity = Mathf.Floor(_light.intensity);
    }

    private void DisableLight()
    {
        if (_light.intensity > 0)
            _light.intensity -= _turnTime * Time.deltaTime;
        else _light.intensity = Mathf.Floor(_light.intensity);
    }

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }
}
