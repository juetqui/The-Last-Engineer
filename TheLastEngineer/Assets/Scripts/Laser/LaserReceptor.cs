using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class LaserReceptor : MonoBehaviour, ILaserReceptor
{
    public UnityEvent OnEndHit;
    public UnityEvent OnHit;
    public UnityEvent OnCompleated;
    public UnityEvent OnDepleated;
    private MeshRenderer _myMeshRenderer = default;
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
    public float _timeToUnfill = 0;
    public float timeModifier;
    public bool _canBeUnfilled= false;

    private void Awake()
    {
        //_myMeshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
        //_myMaterial = _myMeshRenderer.material;
        //_myMaterial.SetFloat("_MinBound", minBound);
        //_myMaterial.SetFloat("_MaxBound", maxBound);
    }
    public void ChargeCompleted()
    {
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
        if (!_isCompleted && !_isCurrentlyLoading)
        {
            _isCurrentlyUnloading = false;
            _isCurrentlyLoading = true;
            StartCoroutine(LoadRoutine(fillTime));
        }
    }
    public void TurnOffObject()
    {
        //_myMeshRenderer.enabled = false;
        _collider.enabled = false;
    }
    public void LaserNotRecived()
    {
        if (!_isCompleted||_canBeUnfilled)
        {
            OnEndHit?.Invoke();
            if ((!_isCompleted && !_isCurrentlyUnloading) )
            {
                _isCurrentlyLoading = false;
                _isCurrentlyUnloading = true;
                StartCoroutine(UnLoadRoutine(unfillTime));
            }
        }
    }
    //public void SetCompleated()
    //{
    //    _isCompleted = true;
    //}
    private IEnumerator LoadRoutine(float loadTime)
    {
        while (_currentLoad <= 1f)
        {
            if (_isCurrentlyLoading == true && _isCurrentlyUnloading == false)
            {
                //print("filleando");
                if (PlayerTDController.Instance.HasNode()&&PlayerTDController.Instance.GetCurrentNodeType()==NodeType.Corrupted)
                {
                    _currentLoad = _currentLoad + Time.deltaTime / loadTime / timeModifier;
                }
                else
                {
                    _currentLoad = _currentLoad + Time.deltaTime / loadTime;

                }
                //_myMaterial.SetFloat("_Step", _currentLoad);

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
    float TimeModifierController()
    {
        return default;
    }
    private IEnumerator UnLoadRoutine(float unloadTime)
    {
        while (_currentLoad >= 0f)
        {
            if (_isCurrentlyLoading == false && _isCurrentlyUnloading == true)
            {

                //print("unfilleando");
                
                    if (PlayerTDController.Instance.HasNode() && PlayerTDController.Instance.GetCurrentNodeType() == NodeType.Corrupted)
                    {
                        _currentLoad = _currentLoad - Time.deltaTime / unloadTime / timeModifier;
                    }
                    else
                    {
                        _currentLoad = _currentLoad - Time.deltaTime / unloadTime;
                    }

                
     
                //_myMaterial.SetFloat("_Step", _currentLoad);
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
