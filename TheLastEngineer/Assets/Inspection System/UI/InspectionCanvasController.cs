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
            _inspectionable = null;
            _canvas.enabled = false;
            return;
        }

        _inspectionable = (Inspectionable)target;
        _inspectionable.OnFinished += CorruptionCleaned;

        _canvas.enabled = true;
        GamepadCursor.Instance.CenterCursor();
    }

    private void CorruptionCleaned()
    {
        _inspectionable.OnFinished -= CorruptionCleaned;
        TargetSelected(null);
    }
}
