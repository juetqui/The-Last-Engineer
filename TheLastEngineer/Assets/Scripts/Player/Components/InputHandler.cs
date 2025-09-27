using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public event Action<Vector2> OnMove = delegate { };
    public event Action OnDash = delegate { };
    public event Action OnInteractStart = delegate { };
    public event Action OnInteractCancel = delegate { };
    public event Action OnCorruptionChange = delegate { };
    public event Action OnCancelSelect = delegate { };

    private InputManager _inputManager = default;

    private void Start()
    {
        _inputManager = InputManager.Instance;
        EnableInputs();
    }

    public void EnableInputs()
    {
        if (_inputManager == null) return;

        _inputManager.dashInput.performed += DashPerformed;
        _inputManager.interactInput.started += InteractStarted;
        _inputManager.interactInput.canceled += InteractCanceled;
        _inputManager.cancelInput.performed += CancelPerformed;
    }

    public void DisableInputs()
    {
        if (_inputManager == null) return;

        _inputManager.dashInput.performed -= DashPerformed;
        _inputManager.interactInput.started -= InteractStarted;
        _inputManager.interactInput.canceled -= InteractCanceled;
        _inputManager.cancelInput.performed -= CancelPerformed;
    }

    private void OnDisable()
    {
        DisableInputs();
    }

    private void Update()
    {
        if (_inputManager == null || _inputManager.playerInputs == null || !_inputManager.playerInputs.Player.enabled) return;
        
        Vector2 moveVector = _inputManager.moveInput.ReadValue<Vector2>();
        OnMove?.Invoke(moveVector);
    }

    private void DashPerformed(InputAction.CallbackContext _) => OnDash?.Invoke();
    private void InteractStarted(InputAction.CallbackContext _) => OnInteractStart?.Invoke();
    private void InteractCanceled(InputAction.CallbackContext _) => OnInteractCancel?.Invoke();
    private void CancelPerformed(InputAction.CallbackContext _) => OnCancelSelect?.Invoke();
}
