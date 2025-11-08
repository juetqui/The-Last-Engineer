using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovementController : MonoBehaviour
{
    [Header("Movement Sensitivity")]
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private LeanTweenType _easeType;

    [Header("Offset Limits")]
    [SerializeField] private float _XLimit = 5f;
    [SerializeField] private float _NegativeXLimit = -5f;
    [SerializeField] private float _YLimit = 15f;
    [SerializeField] private float _NegativeYLimit = -15f;

    private CinemachineFreeLook _freeLookCamera;
    private CinemachineComposer _composer;

    private Vector2 _inputOffset = Vector2.zero;
    private Vector3 _currentOffset = Vector3.zero;

    private bool _isResetting = false;

    private void Awake()
    {
        _freeLookCamera = GetComponent<CinemachineFreeLook>();
        _composer = _freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>();
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

        LeanTween.value(gameObject, _currentOffset, Vector3.zero, 0.5f)
            .setEase(_easeType)
            .setOnUpdate((Vector3 valor) =>
            {
                _currentOffset = valor;
                _composer.m_TrackedObjectOffset = _currentOffset;
            })
            .setOnComplete(() =>
            {
                _currentOffset = Vector3.zero;
                _composer.m_TrackedObjectOffset = _currentOffset;
                _isResetting = false;
            });
    }

    private void SetOffset()
    {
        _currentOffset.x += _inputOffset.x * _sensitivity * Time.deltaTime;
        _currentOffset.y += _inputOffset.y * _sensitivity * Time.deltaTime;

        _currentOffset.x = Mathf.Clamp(_currentOffset.x, _NegativeXLimit, _XLimit);
        _currentOffset.y = Mathf.Clamp(_currentOffset.y, _NegativeYLimit, _YLimit);

        _composer.m_TrackedObjectOffset = _currentOffset;
    }
}
