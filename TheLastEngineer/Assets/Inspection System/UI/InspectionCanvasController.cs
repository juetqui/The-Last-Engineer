using Unity.Cinemachine;
using UnityEngine;

public class InspectionCanvasController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineCamera _inspectionCamera;

    private Canvas _canvas;
    private bool _isBlending = false;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        CinemachineCore.CameraActivatedEvent.AddListener(EnableCanvas);
        PlayerController.Instance.OnInteractableSelected += CheckInteractable;
        ScannerController.Instance.OnScanFinished += DisableCanvas;
        
        _canvas.enabled = false;
        ResetCanvas();
    }

    private void LateUpdate()
    {
        if (!_isBlending || _CMBrain.IsBlending) return;
        
        _canvas.enabled = true;
        GamepadCursor.Instance.CenterCursor();
        _isBlending = false;
    }

    private void OnDestroy()
    {
        CinemachineCore.CameraActivatedEvent.RemoveListener(EnableCanvas);
        ScannerController.Instance.OnScanFinished -= DisableCanvas;
    }

    private void CheckInteractable(IInteractable interactable)
    {
        if (interactable != null) return;

        DisableCanvas();
    }

    private void DisableCanvas()
    {
        _canvas.enabled = false;
        ResetCanvas();
    }

    private void EnableCanvas(ICinemachineCamera.ActivationEventParams evt)
    {
        if (!ReferenceEquals(evt.IncomingCamera, _inspectionCamera)) return;

        _isBlending = true;
    }

    private void ResetCanvas()
    {
        GamepadCursor.Instance.CenterCursor();
        InspectionSystem.Instance.ResetRot();
    }
}
