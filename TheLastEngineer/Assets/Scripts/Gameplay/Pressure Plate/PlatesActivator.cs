using System;
using UnityEngine;

public class PlatesActivator : MonoBehaviour
{
    public static PlatesActivator Instance = null;

    public Action<bool> OnActivatePlates;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            OnActivatePlates?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            OnActivatePlates?.Invoke(false);
        }
    }
}
