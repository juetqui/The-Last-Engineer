using UnityEngine;
using UnityEngine.InputSystem;

public class InspectionSystem : MonoBehaviour
{
    public static InspectionSystem Instance;

    [SerializeField] private Transform _inspectedObject;
    [SerializeField] private float _rotSpeed = 100f;
    [SerializeField] private float _gamepadRotSpeed = 300f;
    [SerializeField] private float _resetRotThreshhold = 1f;
    [SerializeField] private float _resetRotSpeed = 0.05f;

    private Vector3 _lastMousePosition = Vector3.zero;
    private Quaternion _initialObjectRot = Quaternion.identity;

    private float _timer = 0f;
    private bool _canRotate = true;
    private bool _isRotatingWithMouse = false;
    private bool _isResettingPos = false;

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

        if (_isResettingPos) ResetPos();
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

        float xRotation = deltaMousePos.y * _rotSpeed * Time.deltaTime;
        float yRotation = -deltaMousePos.x * _rotSpeed * Time.deltaTime;

        _inspectedObject.Rotate(xRotation, yRotation, 0, Space.World);

        _lastMousePosition = currentMousePosition;
    }

    private void ResetRot(InputAction.CallbackContext context)
    {
        ResetCDValues();
    }

    private void RotateObjectForGamepad()
    {
        Vector2 inputValue = InputManager.Instance.rotate.ReadValue<Vector2>();

        float xRotation = inputValue.y * _gamepadRotSpeed * Time.deltaTime;
        float yRotation = -inputValue.x * _gamepadRotSpeed * Time.deltaTime;

        _inspectedObject.Rotate(xRotation, yRotation, 0, Space.World);
    }

    public void SetHoldValues(bool isHolding, bool canRotate)
    {
        _canRotate = canRotate;
        _timer = 0f;
    }

    private void ResetCDValues()
    {
        _isResettingPos = true;
        _canRotate = false;
        _timer = 0f;
    }

    private void ResetPos()
    {
        _timer += Time.deltaTime * _resetRotSpeed;
        _inspectedObject.rotation = Quaternion.Lerp(_inspectedObject.rotation, _initialObjectRot, _timer);

        if (Quaternion.Angle(_inspectedObject.rotation, _initialObjectRot) <= _resetRotThreshhold)
        {
            _inspectedObject.rotation = _initialObjectRot;
            _isResettingPos = false;
            _canRotate = true;
            _timer = 0f;
        }
    }
}