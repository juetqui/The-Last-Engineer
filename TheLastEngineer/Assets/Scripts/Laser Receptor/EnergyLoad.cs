using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class EnergyLoad : MonoBehaviour
{
    public UnityEvent OnLoaded;
    public UnityEvent OnUnloaded;
    public UnityEvent OnLoading;
    public UnityEvent OnUnloading;
    public bool isFirst;
    public bool isLast;
    [SerializeField] private float _currentLoad = 0f;
    [SerializeField] private float _maxLoad = 0f;
    [SerializeField] private float _timeToLoad = 0f;
    [SerializeField] private float _timeToUnload = 0f;
    private bool _isCurrentlyLoading = false;
    private Coroutine _loadCoroutine;
    bool _isCurrentlyUnloading;
    private void Update()
    {
        
    }
    public void StartLoading()
    {
        print("loading");
        if (_isCurrentlyLoading)
        {
            return;
        }
        _isCurrentlyUnloading = false;
        _isCurrentlyLoading = true;
        _loadCoroutine = StartCoroutine(LoadRoutine(_timeToLoad));
    }
    public void StartUnloading()
    {
        print("unloading");
        if (_isCurrentlyUnloading)
        {
            return;
        }
        _isCurrentlyLoading = false;
        _isCurrentlyUnloading = true;
        _loadCoroutine = StartCoroutine(UnLoadRoutine(_timeToUnload));
    }
    public void StopLoading()
    {
        if (!_isCurrentlyLoading && !_isCurrentlyUnloading)
        {
            return;
        }
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        _isCurrentlyUnloading = false;
        _isCurrentlyLoading = false;
    }
    public void Unload()
    {
       
        StopLoading();
        OnUnloaded?.Invoke();
    }
    private void CompleteLoading()
    {
        if (!_isCurrentlyLoading) return;
        _isCurrentlyLoading = false;
        OnLoaded?.Invoke();
    }
    private IEnumerator LoadRoutine(float loadTime)
    {
        while (_currentLoad <= 1f)
        {
            if (_isCurrentlyLoading == true && _isCurrentlyUnloading == false)
            {
                _currentLoad = _currentLoad + Time.deltaTime/loadTime;
                OnLoading?.Invoke();
                yield return null;
            }
            else if (_isCurrentlyLoading == false || _isCurrentlyUnloading== true)
            {
                yield break;
            }
        }
        _currentLoad = 1;
        print("lodeado");
        OnLoaded?.Invoke();

    }
    private IEnumerator UnLoadRoutine(float unloadTime)
    {
        while (_currentLoad >= 0f)
        {
            if (_isCurrentlyLoading == false && _isCurrentlyUnloading == true)
            {
                _currentLoad = _currentLoad - Time.deltaTime/unloadTime;
                OnUnloading?.Invoke();

                yield return null;
            }
            else if (_isCurrentlyLoading == true || _isCurrentlyUnloading== false)
            { 
                yield break;
            }
        }
        print("unlodeado");

        _currentLoad = 0;
        OnUnloaded?.Invoke();
    }
    public void TotalUnload()
    {
        _currentLoad = 0;
        _isCurrentlyUnloading = false;
        _isCurrentlyLoading = false;
        OnUnloaded?.Invoke();

    }
}
