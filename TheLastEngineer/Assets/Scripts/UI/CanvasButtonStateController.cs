using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasButtonStateController : MonoBehaviour
{
    private Canvas _canvas = default;

    public Dictionary<Selectable, bool> OriginalTBNValues { get; private set; }

    void Start()
    {
        _canvas = GetComponent<Canvas>();
        OriginalTBNValues = new Dictionary<Selectable, bool>();
        GetOriginalButtonValues();
        EnableButtons();
    }

    public void DisableButtons()
    {
        foreach (var item in OriginalTBNValues)
        {
            item.Key.interactable = false;
        }
    }

    public void EnableButtons()
    {
        foreach (var item in OriginalTBNValues)
        {
            item.Key.interactable = item.Value;
        }
    }

    public void GetOriginalButtonValues()
    {
        OriginalTBNValues.Clear();

        foreach (var item in _canvas.GetComponentsInChildren<Selectable>())
        {
            OriginalTBNValues.Add(item, item.interactable);
        }
    }
}
