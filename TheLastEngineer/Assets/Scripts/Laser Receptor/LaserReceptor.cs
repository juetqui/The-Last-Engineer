using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System;
using UnityEngine.Events;

public class LaserReceptor : MonoBehaviour, ILaserReceptor
{
    [SerializeField] UnityEvent OnEndHit;
    [SerializeField] UnityEvent OnHit;
    [SerializeField] UnityEvent OnFill;
    [SerializeField] UnityEvent OnDepleated;
    MeshRenderer meshRenderer;
    Collider collider;
    public bool _isCompleted;
    public void LaserNotRecived()
    {
        if (!_isCompleted)
        {
            OnEndHit?.Invoke();
            //_isCompleted = true;
            //print(_isCompleted);
        }
    }
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();

    }
    public void TurnOffObject()
    {
        meshRenderer.enabled = false;
        collider.enabled = false;
    }
    public void LaserRecived()
    {
        OnHit?.Invoke();
    }
    public void ChargeCompleted()
    {
        OnFill?.Invoke();
    }
    public void ChargeFilled()
    {
        _isCompleted = true;
    }
    public void ChargeDepleted()
    {
        OnDepleated?.Invoke();
    }



    // Update is called once per frame
}
