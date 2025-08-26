using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using UnityEngine.Events;

public class LaserReceptorFill : MonoBehaviour
{
    MeshRenderer _myMeshRenderer;
    Material _myMaterial;
    [SerializeField] float Ymin;
    [SerializeField] float Ymax;
    public bool _completed;
    public UnityEvent OnLoaded;
    public UnityEvent OnUnloaded;
    [SerializeField] float unfillTime;
    [SerializeField] float fillTime;
    float _currentLoad=0;
    private void Start()
    {
        _myMeshRenderer = GetComponent<MeshRenderer>();
        _myMaterial = _myMeshRenderer.material;
        _myMaterial.SetFloat("_Ymin", Ymin);
        _myMaterial.SetFloat("_Ymax", Ymax);

    }
    public void Fill()
    {
        _currentLoad = Mathf.Clamp(_currentLoad + Time.deltaTime / fillTime, 0, 1);
        _myMaterial.SetFloat("_Step", _currentLoad);

    }
    public void SetFull()
    {
        _currentLoad = 1;
        _myMaterial.SetFloat("_Step", _currentLoad);
        _completed = true;

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
            //print(_currentLoad);
        }
        

    }

}
