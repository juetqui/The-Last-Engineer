using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CorruptionRemover : MonoBehaviour
{
    public static CorruptionRemover Instance = null;

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private RawImage _inspectRawImage;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private LayerMask _inspectionLayer;
    [SerializeField] private float _holdTimer = 1f;
    [SerializeField] private float _shakeIntenity = 0.25f;

    [Header("DEBUG SETTINGS")]
    [SerializeField] private GameObject _debugObject;
    [SerializeField] private bool _debug = false;

    private Camera _UICamera = default;
    private Corruption _targetCorruption = default;
    private CinemachineImpulseSource _cameraShake = default;

    private float _timer = 0f;
    private bool _isHolding = false;

    public Action<Corruption> OnCorruptionHit = delegate { };
    public Action<float> OnHittingCorruption = delegate { };
    public Action<Corruption> OnCorruptionRemoved = delegate { };

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        _UICamera = GetComponent<Camera>();
        _cameraShake = GetComponent<CinemachineImpulseSource>();
        
        InputManager.Instance.click.started += CheckForCorruptionUI;
        InputManager.Instance.click.canceled += CancelCorruptionRemove;
    }

    private void OnDestroy()
    {
        InputManager.Instance.click.started -= CheckForCorruptionUI;
        InputManager.Instance.click.canceled -= CancelCorruptionRemove;
    }

    private void Update()
    {
        if (_isHolding) StartHoldTimer();
    }

    private void CheckForCorruptionUI(InputAction.CallbackContext context)
    {
        Ray ray = CalculateRay();

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _inspectionLayer) && hit.collider.gameObject.TryGetComponent(out Corruption corruption))
        {
            if (corruption != _targetCorruption)
            {
                _targetCorruption = corruption;
            }
            
            OnCorruptionHit(corruption);
            SetHoldValues(true, false);
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _inspectionLayer) && _debug)
            _debugObject.transform.position = hit.point;
    }

    private Ray CalculateRay()
    {
        Vector2 screenPos = GamepadCursor.Instance.GetCursorPosition();
        Vector3[] corners = new Vector3[4];

        _inspectRawImage.rectTransform.GetWorldCorners(corners);

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_inspectRawImage.rectTransform, screenPos, _canvas.renderMode == RenderMode.ScreenSpaceCamera ? _mainCamera : null, out localPos);
        Rect rect = _inspectRawImage.rectTransform.rect;

        Vector2 viewportPos = new Vector2(
            (localPos.x - rect.xMin) / rect.width,
            (localPos.y - rect.yMin) / rect.height
        );

        return _UICamera.ViewportPointToRay(viewportPos);
    }

    private void CancelCorruptionRemove(InputAction.CallbackContext context)
    {
        SetHoldValues(false, true);
        InputManager.Instance.StopRumble();
        OnCorruptionHit?.Invoke(null);
    }

    private void StartHoldTimer()
    {
        _timer += Time.unscaledDeltaTime;
        InputManager.Instance.RumblePulse(_timer, _timer);

        float t = Mathf.PingPong(_timer, 1f);

        Vector3 velocityA = new Vector3(_timer, -_timer, 0f) * _shakeIntenity;
        Vector3 velocityB = new Vector3(-_timer, _timer, 0f) * _shakeIntenity;
        Vector3 oscillatedVelocity = Vector3.Lerp(velocityA, velocityB, t);

        _cameraShake.GenerateImpulse(oscillatedVelocity);
        OnHittingCorruption?.Invoke(_timer);

        if (_timer >= _holdTimer)
        {
            SetHoldValues(false, true);
            InputManager.Instance.StopRumble();
            OnCorruptionRemoved?.Invoke(_targetCorruption);
        }
    }

    private void SetHoldValues(bool isHolding, bool canRotate)
    {
        _isHolding = isHolding;
        _timer = 0f;
        InspectionSystem.Instance.SetHoldValues(isHolding, canRotate);
    }
}
