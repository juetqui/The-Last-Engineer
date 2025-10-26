using Cinemachine;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineFreeLook _mainCam;
    [SerializeField] private CinemachineFreeLook _targetLockCam;

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
        if (_isBlending)
        {
            if (!_CMBrain.IsBlending)
            {
                _CMBrain.m_CameraActivatedEvent.RemoveListener(EnableOcclusionCulling);
                _mainCamera.useOcclusionCulling = true;
                _isBlending = false;
            }
        }
    }

    private void OnDestroy()
    {
        PlayerController.Instance.OnInteractableSelected -= TargetSelected;
        ScannerController.Instance.OnScanFinished -= CorruptionCleaned;
    }

    private void CorruptionCleaned() => TargetSelected(null);

    private void TargetSelected(IInteractable target)
    {
        if (target == null || target is not Inspectionable)
        {
            _CMBrain.m_CameraActivatedEvent.AddListener(EnableOcclusionCulling);

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

    private void EnableOcclusionCulling(ICinemachineCamera activeCam, ICinemachineCamera previousCam)
    {
        if (!ReferenceEquals(activeCam, _mainCam)) return;

        _isBlending = true;
    }
}
