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
        ScannerController.Instance.OnScanFinished += HandleFinishedInspection;
        InspectionSystem.Instance.enabled = false;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= OnTargetSelected;
        ScannerController.Instance.OnScanFinished -= HandleFinishedInspection;
    }

    private void HandleFinishedInspection() => StopInspection();

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

        _camera.enabled = true;
        InspectionSystem.Instance.enabled = true;
        PlayerController.Instance.SetCanMove(false);
        InputManager.Instance.UpdateActionMap(ActionMaps.UI);
        
        _currentInteractable = interactable;
        _currentInteractable.Interact(PlayerNodeHandler.Instance, out bool succeded);

        _isInspecting = true;
    }

    public void StopInspection(InputAction.CallbackContext context = default)
    {
        if (!_isInspecting) return;

        HandleFinishedInteraction();
    }

    private void HandleFinishedInteraction()
    {
        _camera.enabled = false;
        InspectionSystem.Instance.enabled = false;

        InputManager.Instance.UpdateActionMap(ActionMaps.Player);
        PlayerController.Instance.SetCanMove(true);
        _currentInteractable = null;
        _isInspecting = false;
    }
}