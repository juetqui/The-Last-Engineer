using UnityEngine;
using UnityEngine.Splines;

public class ConnectionModuleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _orbitPs;
    [SerializeField] private ParticleSystem _completedPs;
    [SerializeField] private SplineAnimate _animator;

    private MainTM _mainTM = default;

    private void Start()
    {
        StopFX();
    }

    private void Update()
    {
        if (_mainTM.Running) PlayFX();
    }

    private void StopFX()
    {
        _orbitPs.Stop();
        _completedPs.Stop();
    }

    private void PlayFX()
    {
        if (!_orbitPs.isPlaying) _orbitPs.Play();
        if (!_completedPs.isPlaying) _completedPs.Play();
        _animator.Play();
    }

    public void SetTM(MainTM mainTM)
    {
        _mainTM = mainTM;
    }
}
