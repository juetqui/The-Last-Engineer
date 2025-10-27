using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InspectionSystem : MonoBehaviour
{
    public static InspectionSystem Instance;

    [Header("Rotation Values")]
    [SerializeField] private Transform _inspectedObject;
    [SerializeField] private float _rotSpeed = 100f;
    [SerializeField] private float _gamepadRotSpeed = 300f;

    [Header("Rotation Reset Values")]
    [SerializeField] private float _resetRotThreshold = 1f;
    [SerializeField] private float _resetRotDuration = 1f;
    [SerializeField] private LeanTweenType _resetEasing = LeanTweenType.easeInOutSine;

    private Vector3 _lastMousePosition = Vector3.zero;
    private Quaternion _initialObjectRot = Quaternion.identity;

    private float _timer = 0f;
    private bool _canRotate = true;
    private bool _isRotatingWithMouse = false;
    private LTDescr _currentTween;

    public bool CanRotate { get { return _canRotate; } }

    public Action OnResetRot = delegate { };

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _initialObjectRot = _inspectedObject.rotation;

        InputManager.Instance.resetRot.started += ResetRot;
        InputManager.Instance.rightClick.started += RightClickStarted;
        InputManager.Instance.rightClick.canceled += RightClickCanceled;
    }

    private void OnDestroy()
    {
        InputManager.Instance.resetRot.started -= ResetRot;
        InputManager.Instance.rightClick.started -= RightClickStarted;
        InputManager.Instance.rightClick.canceled -= RightClickCanceled;
    }

    private void Update()
    {
        if (_canRotate && GamepadCursor.Instance.IsUsingGamepad())
            RotateObjectForGamepad();
    }

    private void RightClickStarted(InputAction.CallbackContext context)
    {
        if (!_canRotate || GamepadCursor.Instance.IsUsingGamepad()) return;

        _isRotatingWithMouse = true;
        _lastMousePosition = (Vector3)InputManager.Instance.rotate.ReadValue<Vector2>();
        InputManager.Instance.rotate.performed += OnRotatePerformed;
    }

    private void RightClickCanceled(InputAction.CallbackContext context)
    {
        if (!_canRotate || GamepadCursor.Instance.IsUsingGamepad()) return;

        _isRotatingWithMouse = false;
        InputManager.Instance.rotate.performed -= OnRotatePerformed;
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        if (!_canRotate || !_isRotatingWithMouse) return;

        Vector3 currentMousePosition = (Vector3)InputManager.Instance.rotate.ReadValue<Vector2>();
        Vector3 deltaMousePos = currentMousePosition - _lastMousePosition;

        float xRotation = deltaMousePos.y * _rotSpeed * Time.unscaledDeltaTime;
        float yRotation = -deltaMousePos.x * _rotSpeed * Time.unscaledDeltaTime;

        _inspectedObject.Rotate(xRotation, yRotation, 0, Space.World);

        _lastMousePosition = currentMousePosition;
    }

    public void ResetRot() => ResetPos();

    private void ResetRot(InputAction.CallbackContext context = default)
    {
        ResetPos();
    }

    private void RotateObjectForGamepad()
    {
        if (!_canRotate || !GamepadCursor.Instance.IsUsingGamepad()) return;

        Vector2 inputValue = InputManager.Instance.rotate.ReadValue<Vector2>();

        if (inputValue == Vector2.zero) return;

        float xRotation = inputValue.y * _gamepadRotSpeed * Time.unscaledDeltaTime;
        float yRotation = -inputValue.x * _gamepadRotSpeed * Time.unscaledDeltaTime;

        _inspectedObject.Rotate(xRotation, yRotation, 0, Space.World);
    }

    public void SetHoldValues(bool isHolding, bool canRotate)
    {
        _canRotate = canRotate;
        _timer = 0f;
    }

    private void ResetPos()
    {
        if (_currentTween != null)
            LeanTween.cancel(_inspectedObject.gameObject);

        _canRotate = false;

        _currentTween = LeanTween.rotate(
            _inspectedObject.gameObject,
            _initialObjectRot.eulerAngles,
            _resetRotDuration
        )
        .setEase(_resetEasing)
        .setOnComplete(() =>
        {
            _inspectedObject.rotation = _initialObjectRot;
            _canRotate = true;
            OnResetRot?.Invoke();
        });
    }
}