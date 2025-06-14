using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Materializer : MonoBehaviour, IMaterializable
{
    [Header("<color=#FF00FF>TOGGLE TO START OBJECT (UN)-MATERIALIZED</color>")]
    [SerializeField] private bool _startsEnabled = true;
    [SerializeField] private Color _outlineColor;
    [SerializeField] private int _activeOutlineWidth = 7;

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

        UpdateAppearance(_currentState, SelectionType.Off);

        MaterializeController.Instance.OnMaterialize += Materialize;

        if (ActiveMaterialization.Instance != null)
        {
            ActiveMaterialization.Instance.OnSelectionActivated += ActivateSelection;
            ActiveMaterialization.Instance.OnObjectSelected += CheckSelected;
        }
    }

    private void OnDestroy()
    {
        MaterializeController.Instance.OnMaterialize -= Materialize;

        if (ActiveMaterialization.Instance != null)
        {
            ActiveMaterialization.Instance.OnSelectionActivated -= ActivateSelection;
            ActiveMaterialization.Instance.OnObjectSelected -= CheckSelected;
        }
    }

    public void ToggleMaterialization()
    {
        _currentState = !_currentState;
        UpdateAppearance(_currentState, SelectionType.Off);
    }

    public void Materialize(bool materialize)
    {
        _currentState = !_currentState;
        UpdateAppearance(_currentState, SelectionType.Off);
    }

    private void ActivateSelection(bool isActive)
    {
        UpdateAppearance(_currentState, isActive ? SelectionType.On : SelectionType.Off);
    }

    private void CheckSelected(Materializer selected)
    {
        UpdateAppearance(_currentState, selected == this ? SelectionType.Selected : SelectionType.On);
    }

    private void UpdateAppearance(bool isMaterialized, SelectionType selectionState)
    {
        _collider.isTrigger = !isMaterialized;
        _renderer.shadowCastingMode = isMaterialized ? ShadowCastingMode.On : ShadowCastingMode.Off;

        if (selectionState == SelectionType.On)
        {
            _outline.OutlineWidth = 3;
            _outline.OutlineColor = Color.yellow;
        }
        else if (selectionState == SelectionType.Selected)
        {
            _outline.OutlineWidth = _activeOutlineWidth;
            _outline.OutlineColor = Color.green;
        }
        else
        {
            _outline.OutlineColor = _outlineColor;
            _renderer.material = isMaterialized ? _enabledMat : _disabledMat;
        }

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = isMaterialized;
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
