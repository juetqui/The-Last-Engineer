using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    private CanvasButtonStateController _currentCanvas = default;
    private CanvasButtonStateController _newCanvas = default;

    private Dictionary<Selectable, bool> _originalStates = new Dictionary<Selectable, bool>();

    public void SetNewUITarget(GameObject newTarget)
    {
        if (newTarget == null)
        {
            throw new Exception("There is no target set to the method");
        }

        _currentCanvas = EventSystem.current.firstSelectedGameObject.gameObject.GetComponentInParent<CanvasButtonStateController>();
        _newCanvas = newTarget.GetComponentInParent<CanvasButtonStateController>();

        if (_currentCanvas == null)
            throw new Exception("There is no Canvas in the EventSystem's firstSelectedGameObject");

        if (_newCanvas == null)
            throw new Exception("There is no Canvas in the newTarget object sent through parameters");

        _currentCanvas.DisableButtons();
        _newCanvas.EnableButtons();

        EventSystem.current.firstSelectedGameObject = newTarget;
        EventSystem.current.SetSelectedGameObject(newTarget);
    }
}
