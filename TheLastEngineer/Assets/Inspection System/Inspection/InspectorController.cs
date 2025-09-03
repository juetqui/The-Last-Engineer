using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InspectorController : MonoBehaviour
{
    public static InspectorController Instance;

    private List<UIInspectionable> _inspectionables = default;

    public Action<UIInspectionable> OnTargetEnabled = delegate { };
    
    // REPLANTEAR LA SELECCION DE INTERACTUABLES EN EL JUGADOR PARA QUE RECIBA UN BOOL EL CUAL LE DIGA A ESTE SCRIPT SI DEBE ENCENDER EL OBJETO EN LA UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        TargetSelected(null);
        _inspectionables = new List<UIInspectionable>();
        _inspectionables = GetComponentsInChildren<UIInspectionable>().ToList();

        //Player.Instance.OnTargetSelected += TargetSelected;
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

            if (enableTarget != null && enableTarget.Type == item.Type)
            {
                item.gameObject.SetActive(true);
                OnTargetEnabled?.Invoke(item);
            }
        }
    }
}
