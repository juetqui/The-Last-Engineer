using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance = default;
    [HideInInspector] public PlayerInput playerInput = default;
    [HideInInspector] public PlayerInputs playerInputs = default;
    [HideInInspector] public InputAction moveInput = default, interactInput = default, dashInput = default, shieldInput = default, corruptionChangeInput = default, modoIzq = default, modoDer = default;

    private Gamepad _gamepad = default;

    public delegate void OnInputsEnabled();
    public event OnInputsEnabled onInputsEnabled;

    public delegate void OnInputsDisabled();
    public event OnInputsDisabled onInputsDisabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        playerInputs = new PlayerInputs();
        playerInput = GetComponent<PlayerInput>();
    }

    public void OnEnable()
    {
        playerInputs.Player.Move.Enable();
        playerInputs.Player.Dash.Enable();
        playerInputs.Player.Shield.Enable();
        playerInputs.Player.CorruptionChange.Enable();
        playerInputs.Player.Interact.Enable();
        playerInputs.Player.ModoDetectiveIzq.Enable();
        playerInputs.Player.ModoDetectiveDer.Enable();

        moveInput = playerInputs.Player.Move;
        dashInput = playerInputs.Player.Dash;
        shieldInput = playerInputs.Player.Shield;
        corruptionChangeInput = playerInputs.Player.CorruptionChange;
        interactInput = playerInputs.Player.Interact;
        modoIzq= playerInputs.Player.ModoDetectiveIzq;
        modoDer = playerInputs.Player.ModoDetectiveDer;
        onInputsEnabled?.Invoke();
    }

    public void OnDisable()
    {
        onInputsDisabled?.Invoke();

        playerInputs.Player.Move.Disable();
        playerInputs.Player.Dash.Disable();
        playerInputs.Player.Interact.Disable();
        playerInputs.Player.Shield.Disable();
        playerInputs.Player.CorruptionChange.Disable();
        playerInputs.Player.ModoDetectiveIzq.Disable();
        playerInputs.Player.ModoDetectiveDer.Disable();

    }

    private void Start()
    {
        onInputsEnabled?.Invoke();
    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration)
    {
        if (playerInput.currentControlScheme != "Gamepad") return;

        _gamepad = Gamepad.current;

        if (_gamepad != null)
        {
            _gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            StartCoroutine(StopRumble(duration));
        }
    }

    private IEnumerator StopRumble(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }

        _gamepad.SetMotorSpeeds(0f, 0f);
    }
}
