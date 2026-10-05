using Unity.Cinemachine;
using UnityEngine;

public class UpdateCameras : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineCamera _mainCam;
    [SerializeField] private CinemachineCamera _targetLockCam;

    private Camera _mainCamera = default;
    private CinemachineInputAxisController _mainCamInput = default;
    private bool _isBlending = false;

    void Start()
    {
        _mainCamera = _CMBrain.GetComponent<Camera>();
        _mainCamInput = _mainCam.GetComponent<CinemachineInputAxisController>();

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
            SetMainCamInput(true);

            return;
        }

        _mainCamera.useOcclusionCulling = false;
        SetMainCamInput(false);

        _targetLockCam.Follow = target.Transform;
        _targetLockCam.LookAt = target.Transform;

        _mainCam.Priority = 0;
        _targetLockCam.Priority = 1;
    }

    // El controller lee la acción directo del asset, sin pasar por el cambio de action map a UI: durante la
    // inspección el stick y el mouse (incluido el virtual) rotarían la cámara de gameplay que queda detrás.
    // No se apaga el componente: su OnEnable re-sincroniza los controllers y el editor de Cinemachine les
    // reaplica los valores por defecto (Gain 1/-1 y acciones CM Default), pisando lo configurado en el prefab.
    // Anular la lectura deja intactos gain, acción y estado de cada eje.
    private void SetMainCamInput(bool active)
    {
        if (_mainCamInput != null)
            _mainCamInput.ReadControlValueOverride = active ? null : s_ignoreInput;
    }

    private static readonly CinemachineInputAxisController.Reader.ControlValueReader s_ignoreInput =
        (action, hint, context, defaultReader) => 0f;

    private void EnableOcclusionCulling(ICinemachineCamera.ActivationEventParams evt)
    {
        if (!ReferenceEquals(evt.IncomingCamera, _mainCam)) return;

        _isBlending = true;
    }
}
