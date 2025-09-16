using UnityEngine;

public class InspectionCanvasController : MonoBehaviour
{
    private Canvas _canvas = default;
    private Inspectionable _inspectionable = default;

    void Start()
    {
        _canvas = GetComponent<Canvas>();

        TargetSelected(null);
        PlayerController.Instance.OnInteractableSelected += TargetSelected;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= TargetSelected;
    }

    private void TargetSelected(IInteractable target)
    {
        if (target == null || target is not Inspectionable)
        {
            if (_inspectionable != null)
                _inspectionable.OnFinished -= ClearReferences;

            _canvas.enabled = false;
            return;
        }

        Inspectionable incomingInspectionable = (Inspectionable)target;

        if (incomingInspectionable != _inspectionable)
        {
            _inspectionable = incomingInspectionable;
            _inspectionable.OnFinished += ClearReferences;
        }

        _canvas.enabled = true;
        GamepadCursor.Instance.CenterCursor();
    }

    private void ClearReferences()
    {
        _inspectionable.OnFinished -= ClearReferences;
        TargetSelected(null);
    }
}
