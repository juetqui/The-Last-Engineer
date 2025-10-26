using Cinemachine;
using UnityEngine;

public class InspectionCanvasController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineFreeLook _inspectionCamera;

    private Canvas _canvas = default;

    private bool _isBlending = false;

    void Start()
    {
        _canvas = GetComponent<Canvas>();
        _CMBrain.m_CameraActivatedEvent.AddListener(EnableCanvas);
        PlayerController.Instance.OnInteractableSelected += CheckInteractable;
        ScannerController.Instance.OnScanFinished += DisableCanvas;
        
        _canvas.enabled = false;
        ResetCanvas();

    }

    private void LateUpdate()
    {
        if (_isBlending)
        {
            if (!_CMBrain.IsBlending)
            {
                _canvas.enabled = true;
                GamepadCursor.Instance.CenterCursor();
                _isBlending = false;
            }
        }
    }

    private void OnDestroy()
    {
        _CMBrain.m_CameraActivatedEvent.RemoveListener(EnableCanvas);
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

    private void EnableCanvas(ICinemachineCamera activeCam, ICinemachineCamera previousCam)
    {
        if (!ReferenceEquals(activeCam, _inspectionCamera)) return;

        _isBlending = true;
    }

    private void ResetCanvas()
    {
        GamepadCursor.Instance.CenterCursor();
        InspectionSystem.Instance.ResetRot();
    }
}
