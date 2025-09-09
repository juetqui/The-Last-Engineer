using Cinemachine;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineFreeLook _mainCam;
    [SerializeField] private CinemachineFreeLook _targetLockCam;

    void Start()
    {
        PlayerController.Instance.OnInteractableSelected += OnTargetSelected;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= OnTargetSelected;
    }

    private void OnTargetSelected(IInteractable target)
    {
        if (target != null && target is Inspectionable)
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
