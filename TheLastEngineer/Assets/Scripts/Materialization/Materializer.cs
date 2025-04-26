using System;
using UnityEngine;

public class Materializer : MonoBehaviour, IMaterializable
{
    private Collider _collider = default;
    private MeshRenderer _renderer = default;
    private Material _originalMat = default, _newMat = default, _currentMat = default;

    public static event Action<bool> OnPlayerInsideTrigger;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
        _originalMat = _renderer.material;

        MaterializeController.Instance.OnMaterialize += Materialize;
    }

    public void Materialize(bool materialize, Material material)
    {
        _collider.isTrigger = !materialize;


        if (!materialize)
        {
            _newMat = material;
            _currentMat = _newMat;
        }
        else
        {
            _currentMat = _originalMat;
        }

        _renderer.material = _currentMat;
    }

    public bool IsTrigger()
    {
        return _collider.isTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(false);
        }
    }
}
