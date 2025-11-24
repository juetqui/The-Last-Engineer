using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

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

    [SerializeField] private List<ParticleSystem> _hitPS = new List<ParticleSystem>();

    private AudioSource _audioSource = default;

    private void Awake()
    {
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
                _currentLoad = _currentLoad + Time.deltaTime / loadTime;
                yield return null;
            }
            else if (_isCurrentlyLoading == false || _isCurrentlyUnloading == true)
            {
                yield break;
            }
        }
        _currentLoad = 1;
        ChargeCompleted();
    }

    private IEnumerator UnLoadRoutine(float unloadTime)
    {
        while (_currentLoad >= 0f)
        {
            if (_isCurrentlyLoading == false && _isCurrentlyUnloading == true)
            {
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
