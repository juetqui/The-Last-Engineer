using UnityEngine;
using UnityEngine.InputSystem;

public class InspectionPlayerManager : MonoBehaviour
{
    private Camera _camera = default;
    private Inspectionable _currentInteractable = default;
    private bool _isInspecting = false;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _camera.enabled = false;
        PlayerController.Instance.OnInteractableSelected += OnTargetSelected;
        InspectionSystem.Instance.enabled = false;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= OnTargetSelected;
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

        //Time.timeScale = 0;
        _camera.enabled = true;
        InspectionSystem.Instance.enabled = true;
        InputManager.Instance.UpdateActionMap(ActionMaps.UI);
        _currentInteractable = interactable;

        _currentInteractable.OnFinished += HandleFinishedInteraction;
        _currentInteractable.Interact(PlayerNodeHandler.Instance, out bool succeded);

        _isInspecting = true;
    }

    public void StopInspection(InputAction.CallbackContext context = default)
    {
        if (!_isInspecting) return;

        //Time.timeScale = 1;
        _camera.enabled = false;
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