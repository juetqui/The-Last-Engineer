using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Materializer : MonoBehaviour, IMaterializable
{
    [Header("<color=#FF00FF>TOGGLE TO START OBJECT (UN)-MATERIALIZED</color>")]
    [SerializeField] private bool _startsEnabled = true;
    [SerializeField] private Color _outlineColor;
    [SerializeField] private Color _outlineActiveColor;
    [SerializeField] private Material _selectedMat;
    [SerializeField] private Material _selectionMat;

    private Collider _collider = default;
    private MeshRenderer _renderer = default;
    private Material _enabledMat = default, _disabledMat = default;
    private Outline _outline = default;
    private bool _currentState = false;

    public static event Action<bool> OnPlayerInsideTrigger = delegate { };

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();

        _enabledMat = _renderer.material;
        _disabledMat = Resources.Load<Material>("Materials/M_Shield");
        
        _outline = gameObject.AddComponent<Outline>();
        _outline.OutlineColor = _outlineColor;
        _outline.OutlineWidth = 3;

        _currentState = _startsEnabled;

        UpdateAppearance(_currentState, SelectionType.Canceled);

        MaterializeController.Instance.OnMaterialize += Materialize;

        if (RangeWorldPowers.Instance != null)
        {
            RangeWorldPowers.Instance.OnSelectionActivated += ActivateSelection;
            RangeWorldPowers.Instance.OnObjectSelected += CheckSelected;
        }
    }

    private void OnDestroy()
    {
        MaterializeController.Instance.OnMaterialize -= Materialize;

        if (RangeWorldPowers.Instance != null)
        {
            RangeWorldPowers.Instance.OnSelectionActivated -= ActivateSelection;
            RangeWorldPowers.Instance.OnObjectSelected -= CheckSelected;
        }
    }

    public void ToggleMaterialization()
    {
        _currentState = !_currentState;
        UpdateAppearance(_currentState, SelectionType.Canceled);
    }

    public void Materialize(bool materialize)
    {
        _currentState = !_currentState;
        UpdateAppearance(_currentState, SelectionType.Canceled);
    }

    private void ActivateSelection(bool isActive)
    {
        UpdateAppearance(_currentState, isActive ? SelectionType.Selecting : SelectionType.Canceled);
    }

    private void CheckSelected(Materializer selected)
    {
        UpdateAppearance(_currentState, selected == this ? SelectionType.Selected : SelectionType.Selecting);
    }

    private void UpdateAppearance(bool isMaterialized, SelectionType selectionState)
    {
        _collider.isTrigger = !isMaterialized;
        _renderer.shadowCastingMode = isMaterialized ? ShadowCastingMode.On : ShadowCastingMode.Off;
        _outline.enabled = isMaterialized;

        if (selectionState == SelectionType.Selecting)
            _renderer.material = _selectionMat;
        else if (selectionState == SelectionType.Selected)
            _renderer.material = _selectedMat;
        else
            _renderer.material = isMaterialized ? _enabledMat : _disabledMat;
    }

    public bool IsTrigger()
    {
        return _collider.isTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collider.isTrigger && other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_collider.isTrigger && other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(false);
        }
    }
}
