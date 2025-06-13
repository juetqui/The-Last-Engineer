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
    public bool _isCompleted;
    public void LaserNotRecived()
    {
        if (!_isCompleted)
        {
            OnEndHit?.Invoke();
            //_isCompleted = true;
            print(_isCompleted);
        }
    }

    public void LaserRecived()
    {
        OnHit?.Invoke();
    }
    public void ChargeCompleted()
    {
        OnFill?.Invoke();
    }
    public void ChargeDepleted()
    {
        OnDepleated?.Invoke();
    }



    // Update is called once per frame
}
