using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static UnityEngine.Rendering.DebugUI;
using DG.Tweening;

public class LaserReceptor : MonoBehaviour, ILaserReceptor
{
    public UnityEvent OnEndHit;
    public UnityEvent OnHit;
    public UnityEvent OnCompleated;
    public UnityEvent OnDepleated;

    private Collider _collider = default;
    public bool _isCompleted = false;
    Material _myMaterial;
    [SerializeField] float minBound;
    [SerializeField] float maxBound;
    public bool _completed;
    [SerializeField] float unfillTime;
    [SerializeField] float fillTime;
    public float _currentLoad = 0;
    bool _isCurrentlyLoading;
    bool _isCurrentlyUnloading;
    //public float _timeToUnfill = 0;
    //public float timeModifier;
    public bool _canBeUnfilled= false;

    private CancellationTokenSource _cancelSource = default;
    private Renderer _renderer = default;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private ParticleSystem _completeFillPartcle;
    [SerializeField] private List<ParticleSystem> _FinishFillParticle;

    [SerializeField] private List<ParticleSystem> _hitPS = new List<ParticleSystem>();


    private AudioSource _audioSource = default;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetFloat("_GlowStep", 0f);
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
        _hitPS = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
    }

    public void ChargeCompleted()
    {
        _audioSource.Stop();
        
        foreach (var ps in _hitPS)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        OnCompleated?.Invoke();
        _isCompleted = true;
    }
    
    public void SetUnCompleted()
    {
        _isCompleted = false;
    }
    
    public void ChargeDepleted()
    {
        OnDepleated?.Invoke();
    }

    public void LaserRecived()
    {
        OnHit?.Invoke();
        
        if (_isCurrentlyUnloading||(!_isCompleted && !_isCurrentlyLoading))
        {
            _isCurrentlyUnloading = false;
            _isCurrentlyLoading = true;
            StartCoroutine(LoadRoutine(fillTime));
        }
    }

    public void TurnOffObject()
    {
        _collider.enabled = false;
    }

    public void LaserNotRecived()
    {
        _completeFillPartcle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _audioSource.Stop();
        foreach (var ps in _hitPS)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (!_isCompleted||_canBeUnfilled)
        {
            OnEndHit?.Invoke();
            //if ((!_isCompleted && !_isCurrentlyUnloading))
            if (!_isCurrentlyUnloading) 
            {
                _isCurrentlyLoading = false;
                _isCurrentlyUnloading = true;
                StartCoroutine(UnLoadRoutine(unfillTime));
            }
        }
    }

    public void Fill()
    {
        StartFill(1f);
    }

    public void Empty()
    {
        StartFill(0f);
    }

    private void StartFill(float target)
    {
        _cancelSource?.Cancel();
        _cancelSource = new CancellationTokenSource();

        _ = AnimateFill(target, _cancelSource.Token);
    }

    private async Task AnimateFill(float target, CancellationToken token)
    {
        float startValue = _renderer.material.GetFloat("_GlowStep");
        float time = 0f;

        while (time < duration)
        {
            if (token.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = time / duration;

            float value = Mathf.Lerp(startValue, target, t);
            _renderer.material.SetFloat("_GlowStep", value);

            await Task.Yield();
        }

        _renderer.material.SetFloat("_GlowStep", target);
    }

    private IEnumerator LoadRoutine(float loadTime)
    {
        _audioSource.Stop();
        if(!_isCompleted)
        _audioSource.Play();
        foreach (var ps in _hitPS)
        {
            ps.Play();
        }
        while (_currentLoad <= 1f)
        {
            if (_isCurrentlyLoading == true && _isCurrentlyUnloading == false)
            {
                Fill();
                _currentLoad = _currentLoad + Time.deltaTime / loadTime;
                yield return null;
            }
            else if (_isCurrentlyLoading == false || _isCurrentlyUnloading == true)
            {
                yield break;
            }
        }

        foreach (var ps in _FinishFillParticle)
        {
            ps.Play();
        }
        _completeFillPartcle.Play();
        _currentLoad = 1;
        ChargeCompleted();
    }

    private IEnumerator UnLoadRoutine(float unloadTime)
    {
        while (_currentLoad >= 0f)
        {
            if (_isCurrentlyLoading == false && _isCurrentlyUnloading == true)
            {
                Empty();
                _currentLoad = _currentLoad - Time.deltaTime / unloadTime;
                yield return null;
            }
            else if (_isCurrentlyLoading == true || _isCurrentlyUnloading == false)
            {
                yield break;
            }
        }
        
        _currentLoad = 0;
        ChargeDepleted();
    }
}
