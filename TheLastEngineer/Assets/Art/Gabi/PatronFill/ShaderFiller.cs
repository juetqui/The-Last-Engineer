using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using UnityEngine.Events;


public class ShaderFiller : MonoBehaviour
{
    MeshRenderer _myMeshRenderer;
    Material _myMaterial;
    [SerializeField] float Ymin;
    [SerializeField] float Ymax;
    public bool _completed;
    public bool _isLoading;
    public bool _isUnloading;
    public UnityEvent FinishLoad;
    public UnityEvent FinishUnload;
    public UnityEvent OnLoading;
    public UnityEvent OnUnLoading;
    [SerializeField] float fillTime;
    [SerializeField] float unfillTime;
    private Coroutine _loadCoroutine;
    public bool startactive;

    float _currentLoad = 0;
    private void Start()
    {
        if (unfillTime == default) unfillTime = fillTime;
        _myMeshRenderer = GetComponent<MeshRenderer>();
        _myMaterial = _myMeshRenderer.material;
        _myMaterial.SetFloat("_Ymin", Ymin);
        _myMaterial.SetFloat("_Ymax", Ymax);
        if (startactive) StartFill();
    }
    public void StartFill()
    {
        _isUnloading = false;
        _isLoading = true;
       // OnLoading?.Invoke();
        _loadCoroutine = StartCoroutine(LoadRoutine(fillTime));
    }
    private IEnumerator LoadRoutine(float loadTime)
    {
        while (_currentLoad <= 1f)
        {
            if (_isLoading == true)
            {
                _currentLoad = _currentLoad + Time.deltaTime / loadTime;
                _myMaterial.SetFloat("_Step", _currentLoad);
                OnLoading?.Invoke();
                yield return null;
            }
            else if (_isUnloading == true)
            {
                _isLoading = false;
                yield break;
            }
        }
        _isLoading = false;
        _currentLoad = 1;
        FinishLoad?.Invoke();
    }
    public void StartUnFill()
    {
        _isLoading = false;
        _isUnloading = true;
        _loadCoroutine = StartCoroutine(UnLoadRoutine(unfillTime));
    }
    private IEnumerator UnLoadRoutine(float unloadTime)
    {
        while (_currentLoad >= 0f)
        {
            if (_isUnloading == true)
            {
                _currentLoad = _currentLoad - Time.deltaTime / unloadTime;
                _myMaterial.SetFloat("_Step", _currentLoad);
                OnUnLoading?.Invoke();
                yield return null;
            }
            else if (_isLoading == true)
            {
                _isUnloading = false;
                yield break;
            }
        }
        _isUnloading=false;
        _currentLoad = 0;
        FinishUnload?.Invoke();
    }
}
