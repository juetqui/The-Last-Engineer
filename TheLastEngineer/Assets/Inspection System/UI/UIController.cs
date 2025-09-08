using UnityEngine;

public class UIController : MonoBehaviour
{
    private Canvas _canvas = default;

    void Start()
    {
        _canvas = GetComponent<Canvas>();

        OnTargetSelected(null);
        PlayerController.Instance.OnInteractableSelected += OnTargetSelected;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= OnTargetSelected;
    }

    private void OnTargetSelected(IInteractable target)
    {
        if (target == null || target is not Inspectionable)
        {
            _canvas.enabled = false;
            return;
        }

        _canvas.enabled = true;
        GamepadCursor.Instance.CenterCursor();
    }
}
