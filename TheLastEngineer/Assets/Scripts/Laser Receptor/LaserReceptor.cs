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
    public void LaserNotRecived()
    {
        OnEndHit.Invoke();
    }

    public void LaserRecived()
    {
        OnHit.Invoke();
    }

  

    // Update is called once per frame
}
