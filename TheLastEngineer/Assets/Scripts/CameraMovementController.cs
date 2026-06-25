using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class CameraMovementController : MonoBehaviour
{
    [Header("Movement Sensitivity")]
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private Ease _easeType;

    [Header("Offset Limits")]
    [SerializeField] private float _horizontalLimit = 5f;
    [SerializeField] private float _YLimit = 15f;
    [SerializeField] private float _NegativeYLimit = -15f;

    private CinemachineCamera _freeLookCamera;
    private CinemachineOrbitalFollow _orbitalFollow;
    private CinemachineRotationComposer _composer;

    private Vector2 _inputOffset = Vector2.zero;
    private Vector3 _currentOffset = Vector3.zero;

    private bool _isResetting = false;

    private void Awake()
    {
        _freeLookCamera = GetComponent<CinemachineCamera>();
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _composer = GetComponent<CinemachineRotationComposer>();
    }

    private void Start()
    {
        InputManager.Instance.rotateInput.performed += OnLook;
        InputManager.Instance.rotateInput.canceled += OnReset;

        OnReset();
    }

    private void LateUpdate()
    {
        if (_isResetting) return;

        if (_inputOffset != Vector2.zero)
            SetOffset();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        _inputOffset = context.ReadValue<Vector2>().normalized;
    }

    private void OnReset(InputAction.CallbackContext context = default)
    {
        _inputOffset = Vector2.zero;
        _isResetting = true;

        Tween.Custom(gameObject, _currentOffset, Vector3.zero, 0.5f, (_, valor) =>
            {
                _currentOffset = valor;
                _composer.TargetOffset = _currentOffset;
            }, _easeType)
            .OnComplete(() =>
            {
                _currentOffset = Vector3.zero;
                _composer.TargetOffset = _currentOffset;
                _isResetting = false;
            });
    }

    private void SetOffset()
    {
        var yaw = _orbitalFollow.HorizontalAxis.Value * Mathf.Deg2Rad;
        var horizontalDelta = _inputOffset.x * _sensitivity * Time.deltaTime;

        _currentOffset.x += horizontalDelta * Mathf.Cos(yaw);
        _currentOffset.z -= horizontalDelta * Mathf.Sin(yaw);
        _currentOffset.y += _inputOffset.y * _sensitivity * Time.deltaTime;

        // Clamp horizontal magnitude (circular limit in XZ plane)
        Vector2 horizontal = new Vector2(_currentOffset.x, _currentOffset.z);
        if (horizontal.magnitude > _horizontalLimit)
        {
            horizontal = horizontal.normalized * _horizontalLimit;
            _currentOffset.x = horizontal.x;
            _currentOffset.z = horizontal.y;
        }

        _currentOffset.y = Mathf.Clamp(_currentOffset.y, _NegativeYLimit, _YLimit);

        _composer.TargetOffset = _currentOffset;
    }
}
