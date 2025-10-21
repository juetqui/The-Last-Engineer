using Cinemachine;
using UnityEngine;

public class InspectionCanvasController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain _CMBrain;
    [SerializeField] private CinemachineFreeLook _inspectionCamera;

    private Canvas _canvas = default;
    private Inspectionable _inspectionable = default;

    private bool _isBlending = false;

    void Start()
    {
        _canvas = GetComponent<Canvas>();
        _CMBrain.m_CameraActivatedEvent.AddListener(EnableCanvas);
        
        _canvas.enabled = true;
        ResetCanvas();

        TargetSelected(null);
        PlayerController.Instance.OnInteractableSelected += TargetSelected;
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
        PlayerController.Instance.OnInteractableSelected -= TargetSelected;
        _CMBrain.m_CameraActivatedEvent.RemoveListener(EnableCanvas);
    }

    private void TargetSelected(IInteractable target)
    {
        if (target == null || target is not Inspectionable)
        {
            if (_inspectionable != null)
                _inspectionable.OnFinished -= ClearReferences;

            _canvas.enabled = false;
            ResetCanvas();
            return;
        }

        Inspectionable incomingInspectionable = (Inspectionable)target;

        if (incomingInspectionable != _inspectionable)
        {
            _inspectionable = incomingInspectionable;
            _inspectionable.OnFinished += ClearReferences;
        }
    }

    private void ClearReferences()
    {
        _inspectionable.OnFinished -= ClearReferences;
        TargetSelected(null);
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
