using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserActivator : MonoBehaviour, ILaserReceptor
{
    private List<LaserController> _lasers = default;

    public LaserController Laser { get; private set; }

    private void Awake()
    {
        _lasers = GetComponentsInChildren<LaserController>().ToList();
        Laser = _lasers.First();
    }

    public void LaserReceived()
    {
        foreach (var laser in _lasers)
        {
            laser.LaserReceived();
        }
    }

    public void LaserNotReceived()
    {
        foreach (var laser in _lasers)
        {
            laser.LaserNotReceived();
        }
    }
}
