using Cinemachine;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineFreeLook _mainCam;
    [SerializeField] private CinemachineFreeLook _targetLockCam;

    private bool _camIsLeaving = false;

    void Start()
    {
        PlayerTDController.Instance.OnInteractableSelected += OnTargetSelected;
        _CMBrain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
    }

    private void OnDestroy()
    {
        PlayerTDController.Instance.OnInteractableSelected -= OnTargetSelected;
        _CMBrain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
    }

    private void OnCameraActivated(ICinemachineCamera newCamera, ICinemachineCamera oldCamera)
    {
        return;
        //if (_camIsLeaving)
        //    Time.timeScale = 1f;
        //else
        //    Time.timeScale = 0f;
    }

    private void OnTargetSelected(IInteractable target)
    {
        if (target != null && target is Inspectionable)
        {
            _camIsLeaving = true;
            _targetLockCam.Follow = target.Transform;
            _targetLockCam.LookAt = target.Transform;

            _mainCam.Priority = 0;
            _targetLockCam.Priority = 1;
            
            return;
        }

        _camIsLeaving = false;
        _targetLockCam.Follow = null;
        _targetLockCam.LookAt = null;

        _mainCam.Priority = 1;
        _targetLockCam.Priority = 0;
    }
}
