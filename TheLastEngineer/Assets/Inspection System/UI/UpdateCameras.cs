using Unity.Cinemachine;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineCamera _mainCam;
    [SerializeField] private CinemachineCamera _targetLockCam;

    private Camera _mainCamera = default;
    private bool _isBlending = false;

    void Start()
    {
        _mainCamera = _CMBrain.GetComponent<Camera>();

        PlayerController.Instance.OnInteractableSelected += TargetSelected;
        ScannerController.Instance.OnScanFinished += CorruptionCleaned;
    }

    private void Update()
    {
        if (!_isBlending) return;
        if (_CMBrain.IsBlending) return;

        CinemachineCore.CameraActivatedEvent.RemoveListener(EnableOcclusionCulling);
        _mainCamera.useOcclusionCulling = true;
        _isBlending = false;
    }

    private void OnDestroy()
    {
        if (!this.isActiveAndEnabled) return;

        PlayerController.Instance.OnInteractableSelected -= TargetSelected;
        ScannerController.Instance.OnScanFinished -= CorruptionCleaned;
    }

    private void CorruptionCleaned() => TargetSelected(null);

    private void TargetSelected(IInteractable target)
    {
        if (target == null || target is not Inspectionable)
        {
            CinemachineCore.CameraActivatedEvent.AddListener(EnableOcclusionCulling);

            _targetLockCam.Follow = null;
            _targetLockCam.LookAt = null;

            _mainCam.Priority = 1;
            _targetLockCam.Priority = 0;

            return;
        }

        _mainCamera.useOcclusionCulling = false;

        _targetLockCam.Follow = target.Transform;
        _targetLockCam.LookAt = target.Transform;

        _mainCam.Priority = 0;
        _targetLockCam.Priority = 1;
    }

    private void EnableOcclusionCulling(ICinemachineCamera.ActivationEventParams evt)
    {
        if (!ReferenceEquals(evt.IncomingCamera, _mainCam)) return;

        _isBlending = true;
    }
}
