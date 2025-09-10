using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance = default;

    [HideInInspector] public PlayerInput playerInput = default;
    [HideInInspector] public PlayerInputs playerInputs = default;
    
    #region PLAYER INPUTS
    [HideInInspector] public InputAction moveInput = default;
    [HideInInspector] public InputAction interactInput = default;
    [HideInInspector] public InputAction dashInput = default;
    [HideInInspector] public InputAction corruptionChangeInput = default;
    [HideInInspector] public InputAction pauseInput = default;
    #endregion

    #region UI INPUTS
    [HideInInspector] public InputAction rotate = default;
    [HideInInspector] public InputAction click = default;
    [HideInInspector] public InputAction rightClick = default;
    [HideInInspector] public InputAction resetRot = default;
    [HideInInspector] public InputAction cancelInput = default;
    #endregion

    private Gamepad _gamepad = default;

    public Action OnInputsEnabled = delegate { };
    public Action OnInputsDisabled = delegate { };

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
        playerInputs.Player.Enable();

        moveInput = playerInputs.Player.Move;
        dashInput = playerInputs.Player.Dash;
        corruptionChangeInput = playerInputs.Player.CorruptionChange;
        interactInput = playerInputs.Player.Interact;
        pauseInput = playerInputs.Player.Pause;

        rotate = playerInputs.UI.Rotate;
        click = playerInputs.UI.Click;
        rightClick = playerInputs.UI.RightClick;
        resetRot = playerInputs.UI.ResetRotation;
        cancelInput = playerInputs.UI.Cancel;

        OnInputsEnabled?.Invoke();
    }

    public void OnDisable()
    {
        OnInputsDisabled?.Invoke();
        playerInputs.Player.Disable();
    }

    private void Start()
    {
        OnInputsEnabled?.Invoke();
    }
    
    public void UpdateActionMap(ActionMaps newActionMap)
    {
        playerInput.SwitchCurrentActionMap(newActionMap.ToString());

        if (newActionMap == ActionMaps.Player)
        {
            playerInputs.UI.Disable();
            playerInputs.Player.Enable();
        }
        else
        {
            playerInputs.Player.Disable();
            playerInputs.UI.Enable();
        }
    }

    public void SetControlScheme(string scheme)
    {
        playerInputs.bindingMask = InputBinding.MaskByGroup(scheme);
    }

    private bool IsUsingGamepad()
    {
        _gamepad = Gamepad.current;
        
        bool hasGamepadScheme = playerInput.currentControlScheme == "Gamepad";
        bool hasGamepad = _gamepad != null;

        return hasGamepadScheme && hasGamepad;
    }

    public void RumblePulse(float lowFrequency, float highFrequency, float duration)
    {
        if (!IsUsingGamepad()) return;
            
        _gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        StartCoroutine(StopRumble(duration));
    }

    public void RumblePulse(float lowFrequency, float highFrequency)
    {
        if (!IsUsingGamepad()) return;
            
        _gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
    }

    public void StopRumble()
    {
        if (!IsUsingGamepad()) return;

        _gamepad.SetMotorSpeeds(0f, 0f);
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

public enum ActionMaps
{
    Player,
    UI
}
