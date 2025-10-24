using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InspectorController : MonoBehaviour
{
    public static InspectorController Instance;

    private List<UIInspectionable> _inspectionables = default;
    private CorruptionGenerator _currentGenerator = default;
    private UIInspectionable _currentUIInspectionable = default;

    public Action<UIInspectionable> OnTargetEnabled = delegate { };
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        TargetSelected(null);
        _inspectionables = new List<UIInspectionable>();
        _inspectionables = GetComponentsInChildren<UIInspectionable>().ToList();

        PlayerController.Instance.OnInteractableSelected += TargetSelected;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= TargetSelected;
    }

    private void TargetSelected(IInteractable target)
    {
        if (target is Inspectionable)
        {
            Inspectionable inspectionable = (Inspectionable)target;
            SetUpInspectionables(inspectionable);
        }
    }

    private void SetUpInspectionables(Inspectionable enableTarget = null)
    {
        foreach (var item in _inspectionables)
        {
            item.gameObject.SetActive(false);
        }

        if (enableTarget == null) return;

        foreach (var item in _inspectionables)
        {
            if (enableTarget.Type == item.Type)
            {
                item.gameObject.SetActive(true);
                item.SetUpGenerator(enableTarget.CorruptionGenerator);

                if (_currentGenerator != null)
                    _currentGenerator.OnUpdatedInstances -= HandleGeneratorUpdated;

                _currentGenerator = enableTarget.CorruptionGenerator;
                _currentUIInspectionable = item;

                _currentGenerator.OnUpdatedInstances += HandleGeneratorUpdated;
                _currentGenerator.OnObjectCleaned += enableTarget.CorruptionCleaned;

                OnTargetEnabled?.Invoke(item);
                break;
            }
        }
    }

    private void HandleGeneratorUpdated()
    {
        if (_currentUIInspectionable == null || _currentGenerator == null)
            return;

        _currentGenerator.RefreshCorruptionVisual(_currentUIInspectionable.UICorruption);
    }
}
