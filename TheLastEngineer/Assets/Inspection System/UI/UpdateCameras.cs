using Cinemachine;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook _mainCam;
    [SerializeField] CinemachineFreeLook _targetLockCam;

    // REPLANTEAR LA SELECCION DE INTERACTUABLES EN EL JUGADOR PARA QUE RECIBA UN BOOL EL CUAL LE DIGA A ESTE SCRIPT SI DEBE CAMBIAR DE CAMARAS

    void Start()
    {
        //Player.Instance.OnTargetSelected += OnTargetSelected;
    }

    private void OnTargetSelected(IInteractable target)
    {
        if (target != null)
        {
            _targetLockCam.Follow = target.Transform;
            _targetLockCam.LookAt = target.Transform;

            _mainCam.Priority = 0;
            _targetLockCam.Priority = 1;
            
            return;
        }

        _targetLockCam.Follow = null;
        _targetLockCam.LookAt = null;

        _mainCam.Priority = 1;
        _targetLockCam.Priority = 0;
    }
}
