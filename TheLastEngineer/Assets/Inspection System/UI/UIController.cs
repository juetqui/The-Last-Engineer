using UnityEngine;

public class UIController : MonoBehaviour
{
    private Canvas _canvas = default;

    // REPLANTEAR LA SELECCION DE INTERACTUABLES EN EL JUGADOR PARA QUE RECIBA UN BOOL EL CUAL LE DIGA A ESTE SCRIPT SI DEBE ENCENDER EL CANVAS

    void Start()
    {
        _canvas = GetComponent<Canvas>();

        OnTargetSelected(null);
        //Player.Instance.OnTargetSelected += OnTargetSelected;
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
