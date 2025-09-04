using UnityEngine;
using UnityEngine.InputSystem;

public class InspectionPlayerManager : MonoBehaviour
{
    private Inspectionable _currentInteractable;
    private bool _isInspecting = false;

    void Start()
    {
        PlayerTDController.Instance.OnInteractableSelected += OnTargetSelected;
        InspectionSystem.Instance.enabled = false;
    }

    private void OnDestroy()
    {
        PlayerTDController.Instance.OnInteractableSelected -= OnTargetSelected;
    }

    private void OnTargetSelected(IInteractable target)
    {
        if (target == null || target is not Inspectionable)
            StopInspection();
        else
            StartInspection((Inspectionable)target);
    }

    public void StartInspection(Inspectionable interactable)
    {
        if (interactable == null || _isInspecting) return;

        Time.timeScale = 0;
        InspectionSystem.Instance.enabled = true;
        InputManager.Instance.UpdateActionMap(ActionMaps.UI);
        _currentInteractable = interactable;

        _currentInteractable.OnFinished += HandleFinishedInteraction;
        _currentInteractable.Interact(PlayerTDController.Instance, out bool succeded);

        _isInspecting = true;
    }

    public void StopInspection(InputAction.CallbackContext context = default)
    {
        if (!_isInspecting) return;

        Time.timeScale = 1;
        InspectionSystem.Instance.enabled = false;
        _currentInteractable.OnFinished -= HandleFinishedInteraction;
        _currentInteractable.StopInteraction();
        HandleFinishedInteraction();
    }

    private void HandleFinishedInteraction()
    {
        InputManager.Instance.UpdateActionMap(ActionMaps.Player);
        _currentInteractable = null;
        _isInspecting = false;
    }
}