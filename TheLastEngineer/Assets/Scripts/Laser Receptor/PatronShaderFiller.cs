using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using UnityEngine.Events;

public class PatronShaderFiller : MonoBehaviour
{
    MeshRenderer _myMeshRenderer;
    Material _myMaterial;
    [SerializeField] float minBound;
    [SerializeField] float maxBound;
    public bool _completed;
    [SerializeField] float unfillTime;
    [SerializeField] float fillTime;
    public float _currentLoad=0;
    private void Start()
    {
        _myMeshRenderer = GetComponent<MeshRenderer>();
        _myMaterial = _myMeshRenderer.material;
        _myMaterial.SetFloat("_MinBound", minBound);
        _myMaterial.SetFloat("_MaxBound", maxBound);

    }
    public void Fill()
    {
        _currentLoad = Mathf.Clamp(_currentLoad + Time.deltaTime / fillTime, 0, 1);
        _myMaterial.SetFloat("_Step", _currentLoad);
        print(_currentLoad);

    }
    public void SetFull()
    {
        _currentLoad = 1;
        _myMaterial.SetFloat("_Step", _currentLoad);
        //_completed = true;
        print(_currentLoad);

    }
    public void SetDepleated()
    {
        _currentLoad = 0;
        _myMaterial.SetFloat("_Step", _currentLoad);
        //_completed = false;
        print("medescargo");

    }
    public void UnFill()
    {
        if (!_completed)
        {
            _currentLoad = Mathf.Clamp(_currentLoad - Time.deltaTime / unfillTime, 0, 1);
            _myMaterial.SetFloat("_Step", _currentLoad);
            print(_currentLoad);
        }
        

    }

}
