using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Materializer : MonoBehaviour, IMaterializable
{
    [Header("<color=#FF00FF>TOGGLE TO START OBJECT (UN)-MATERIALIZED</color>")]
    [SerializeField] private bool _startsEnabled = true;
    [SerializeField] private bool _debug = false;
    [SerializeField] private Color _outlineColor;
    [SerializeField] private Color _outlineActiveColor;
    [SerializeField] private Material _selectedMat;
    [SerializeField] private Material _selectionMat;

    private Collider _collider = default;
    private MeshRenderer _renderer = default;
    private Material _enabledMat = default, _disabledMat = default, _currentMat = default;
    private Outline _outline = default;
    private bool _isTrigger = default, _hasChangedState = false, _currentState = false;
    private int _toggleCount = 0;

    public static event Action<bool> OnPlayerInsideTrigger = delegate { };

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
        _isTrigger = _collider.isTrigger;

        _enabledMat = _renderer.material;
        _disabledMat = Resources.Load<Material>("Materials/M_Shield");
        
        _outline = gameObject.AddComponent<Outline>();
        _outline.OutlineColor = _outlineColor;
        _outline.OutlineWidth = 3;

        _currentState = _startsEnabled;

        ChangeMaterialize(_startsEnabled);

        MaterializeController.Instance.OnMaterialize += Materialize;
        RangeWorldPowers.Instance.OnSelectionActivated += ActivateSelection;
        RangeWorldPowers.Instance.OnObjectSelected += CheckSelected;
    }

    private void ActivateSelection(bool isActive)
    {
        if (isActive)
        {
            _renderer.material = _selectionMat;
        }
        else
        {
            _renderer.material = _currentMat;
        }
    }

    private void CheckSelected(Materializer selected)
    {
        if (selected == this)
        {
            _renderer.material = _selectedMat;
        }
        else
        {
            _renderer.material = _currentMat;
        }
    }

    public void ToggleMaterialization()
    {
        _hasChangedState = true;
        _toggleCount++;

        if (_toggleCount % 2 == 0)
        {
            _toggleCount = 0;
        }

        bool targetState = !_currentState;
        ChangeMaterialize(targetState);
    }

    public void Materialize(bool materialize)
    {
        bool targetState;

        if (materialize)
        {
            targetState = !_currentState;
        }
        else
        {
            targetState = _hasChangedState && (_toggleCount % 2 != 0) ? !_startsEnabled : _startsEnabled;
            _currentState = targetState;
            _hasChangedState = false;
        }

        ChangeMaterialize(targetState);
    }

    public void ChangeMaterialize(bool SetMaterialized)
    {
        _collider.isTrigger = !SetMaterialized;
        _isTrigger = _collider.isTrigger;
        _currentState = SetMaterialized;

        if (SetMaterialized)
        {
            _renderer.shadowCastingMode = ShadowCastingMode.On;
            _currentMat = _enabledMat;
            SetOutline(true);
        }
        else
        {
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _currentMat = _disabledMat;
            SetOutline(false);
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

    #region COLLISION CHECKS
    public bool IsTrigger()
    {
        return _collider.isTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(_isTrigger);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(_isTrigger);
        }
    }
    #endregion
}
