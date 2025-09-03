using UnityEngine;
using UnityEngine.InputSystem;

public class InspectionPlayerManager : MonoBehaviour
{
    private InspectionSystem _inspectionSystem;
    private Inspectionable _currentInteractable;
    private bool _isInspecting = false;

    // REPLANTEAR LA SELECCION DE INTERACTUABLES EN EL JUGADOR PARA QUE RECIBA UN BOOL EL CUAL LE DIGA A ESTE SCRIPT SI DEBE CAMBIAR DE CAMARAS O NO

    void Start()
    {
        _inspectionSystem = GetComponent<InspectionSystem>();
        //Player.Instance.OnTargetSelected += OnTargetSelected;
    }

    private void OnDestroy()
    {
        //Player.Instance.OnTargetSelected -= OnTargetSelected;
    }

    // REVISAR LA SUSCRIPCION AL EVENTO DEL PLAYER PARA QUE SE COMPORTE COMO UNA INTERACCION ENVIANDOSE A SI MISMO ENTRE LOS PARAMETROS
    private void OnTargetSelected(IInteractable target, PlayerTDController player)
    {
        if (!(Inspectionable)target) return;

        if (target == null) StopInspection();
        else StartInspection((Inspectionable)target, player);
    }

    public void StartInspection(Inspectionable interactable, PlayerTDController player)
    {
        if (interactable == null || _isInspecting) return;

        InputManager.Instance.UpdateActionMap(ActionMaps.UI);
        _currentInteractable = interactable;

        _currentInteractable.OnFinished += HandleFinishedInteraction;
        _currentInteractable.Interact(player, out bool succeded);

        _isInspecting = succeded;
    }

    public void StopInspection(InputAction.CallbackContext context = default)
    {
        if (!_isInspecting) return;

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