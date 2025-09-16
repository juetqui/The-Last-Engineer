using Cinemachine;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineFreeLook _mainCam;
    [SerializeField] private CinemachineFreeLook _targetLockCam;

    private Inspectionable _inspectionable = default;

    void Start()
    {
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

            _targetLockCam.Follow = null;
            _targetLockCam.LookAt = null;

            _mainCam.Priority = 1;
            _targetLockCam.Priority = 0;

            return;
        }

        _inspectionable = (Inspectionable)target;
        _inspectionable.OnFinished += CorruptionCleaned;

        _targetLockCam.Follow = target.Transform;
        _targetLockCam.LookAt = target.Transform;

        _mainCam.Priority = 0;
        _targetLockCam.Priority = 1;
    }

    private void CorruptionCleaned()
    {
        _inspectionable.OnFinished -= CorruptionCleaned;
        TargetSelected(null);
    }
}
