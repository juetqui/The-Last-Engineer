using System;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Materializer : MonoBehaviour, IMaterializable
{
    [Header("<color=#FF00FF>TOGGLE TO START OBJECT (UN)-MATERIALIZED</color>")]
    [SerializeField] private bool _startsEnabled = true;
    [SerializeField] private Color _outlineColor;

    private Collider _collider = default;
    private MeshRenderer _renderer = default;
    private Material _enabledMat = default, _disabledMat = default, _currentMat = default;
    private Outline _outline = default;

    public static event Action<bool> OnPlayerInsideTrigger;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
        
        _enabledMat = _renderer.material;
        _disabledMat = Resources.Load<Material>("Materials/M_Shield");

        _outline = gameObject.AddComponent<Outline>();
        _outline.OutlineColor = _outlineColor;
        _outline.OutlineWidth = 3;

        if (!_startsEnabled)
        {
            Materialize(!_startsEnabled);
        }
        else
        {
            Materialize(_startsEnabled);
        }


        MaterializeController.Instance.OnMaterialize += Materialize;
    }

    public void Materialize(bool materialize)
    {
        if (!_startsEnabled)
        {
            materialize = !materialize;
        }

        _collider.isTrigger = !materialize;

        if (!materialize)
        {
            _currentMat = _disabledMat;
            SetOutline(false);
        }
        else
        {
            _currentMat = _enabledMat;
            SetOutline(true);
        }

        _renderer.material = _currentMat;
    }

    private void SetOutline(bool activate)
    {
        if (activate)
        {
            _outline.enabled = true;
        }
        else
        {
            _outline.enabled = false;
        }
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
