using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserActivator : MonoBehaviour, ILaserReceptor
{
    private List<Laser> _lasers = default;

    private void Awake()
    {
        _lasers = GetComponentsInChildren<Laser>().ToList();
    }

    public void LaserRecived()
    {
        foreach (var laser in _lasers)
        {
            laser.LaserRecived();
        }
    }

    public void LaserNotRecived()
    {
        foreach (var laser in _lasers)
        {
            laser.LaserNotRecived();
        }
    }
}
