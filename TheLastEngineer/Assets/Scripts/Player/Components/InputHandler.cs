using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public event Action<Vector2> OnMove = delegate { };
    public event Action OnDash = delegate { };
    public event Action OnInteractStart = delegate { };
    public event Action OnInteractCancel = delegate { };
    public event Action OnCancelSelect = delegate { };
    public event Action OnDebug = delegate { };

    // Set gives the node's state to the object, Take pulls it back. Tap only: they fire on press.
    public event Action OnSetStart = delegate { };
    public event Action OnTakeStart = delegate { };

    private InputManager _inputManager = default;

    // EnableInputs se llama desde Start, desde OnRespawned y desde SetCollisions(true), sin un
    // DisableInputs garantizado en el medio. Con interacciones idempotentes eso no se notaba;
    // con Set/Take una doble suscripción haría dos intercambios por pulsación.
    private bool _hooked = false;

    private void Start()
    {
        _inputManager = InputManager.Instance;
        EnableInputs();
    }

    public void EnableInputs()
    {
        if (_inputManager == null || _hooked) return;

        _hooked = true;

        _inputManager.dashInput.performed += DashPerformed;
        _inputManager.interactInput.started += InteractStarted;
        _inputManager.interactInput.canceled += InteractCanceled;
        _inputManager.setInput.started += SetStarted;
        _inputManager.takeInput.started += TakeStarted;
        _inputManager.cancelInput.performed += CancelPerformed;
        _inputManager.debugInput.performed += DebugPerformed;
    }

    public void DisableInputs()
    {
        if (_inputManager == null || !_hooked) return;

        _hooked = false;

        _inputManager.dashInput.performed -= DashPerformed;
        _inputManager.interactInput.started -= InteractStarted;
        _inputManager.interactInput.canceled -= InteractCanceled;
        _inputManager.setInput.started -= SetStarted;
        _inputManager.takeInput.started -= TakeStarted;
        _inputManager.cancelInput.performed -= CancelPerformed;
        _inputManager.debugInput.performed -= DebugPerformed;

        // Al desenganchar en medio de una pulsación el canceled nunca llega, así que lo emitimos
        // nosotros: si no, un hold de Interact seguiría vivo con el jugador muerto o en pausa.
        OnInteractCancel?.Invoke();
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
    private void SetStarted(InputAction.CallbackContext _) => OnSetStart?.Invoke();
    private void TakeStarted(InputAction.CallbackContext _) => OnTakeStart?.Invoke();
    private void CancelPerformed(InputAction.CallbackContext _) => OnCancelSelect?.Invoke();
    private void DebugPerformed(InputAction.CallbackContext _) => OnDebug?.Invoke();
}
