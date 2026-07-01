using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputDeviceDetector", menuName = "Systems/Input Device Detector")]
public class InputDeviceDetector : ScriptableObject
{
    public enum DeviceType
    {
        KeyboardMouse,
        Xbox,
        PlayStation,
        Switch,
        GenericGamepad,
        Unknown
    }

    public event Action<DeviceType> OnDeviceChanged;

    [SerializeField] private DeviceType _currentDevice = DeviceType.Unknown;
    public DeviceType CurrentDevice => _currentDevice;

    private bool _isInitialized;
    private InputAction _anyKeyAction;
    private InputAction _anyGamepadAction;

    public void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // Resetear estado: un ScriptableObject retiene su valor entre sesiones de
        // Play en el editor, y DetectCurrentDevice() hace early-return si el device
        // no es Unknown. Sin este reset, la 2da sesion de Play nunca vuelve a detectar.
        _currentDevice = DeviceType.Unknown;

        // Crear acciones para detectar ANY input
        _anyKeyAction = new InputAction(
            name: "AnyKeyboardMouse",
            type: InputActionType.PassThrough
        );
        
        // Detectar CUALQUIER tecla o mouse
        _anyKeyAction.AddBinding("<Keyboard>/<Button>");
        _anyKeyAction.AddBinding("<Mouse>/<Button>");

        _anyGamepadAction = new InputAction(
            name: "AnyGamepad",
            type: InputActionType.PassThrough
        );
        
        // Detectar CUALQUIER botón o stick del gamepad
        _anyGamepadAction.AddBinding("<Gamepad>/<Button>");
        _anyGamepadAction.AddBinding("<Gamepad>/dpad");

        // Callbacks cuando se detecta input
        _anyKeyAction.performed += ctx => OnKeyboardMouseInput(ctx);
        _anyGamepadAction.performed += ctx => OnGamepadInput(ctx);

        // Activar las acciones
        _anyKeyAction.Enable();
        _anyGamepadAction.Enable();

        // Eventos de conexión/desconexión
        InputSystem.onDeviceChange += OnDeviceChange;

        DetectCurrentDevice();
        
        // Debug.Log("InputDeviceDetector initialized");
    }

    public void Dispose()
    {
        if (!_isInitialized) return;
        _isInitialized = false;

        _anyKeyAction?.Disable();
        _anyKeyAction?.Dispose();
        
        _anyGamepadAction?.Disable();
        _anyGamepadAction?.Dispose();

        InputSystem.onDeviceChange -= OnDeviceChange;

        // Limpiar suscriptores para evitar fugas / dobles callbacks entre sesiones
        // de Play en el editor (el SO y sus delegados sobreviven al stop de Play).
        OnDeviceChanged = null;

        // Debug.Log("InputDeviceDetector disposed");
    }

    private void OnKeyboardMouseInput(InputAction.CallbackContext context)
    {
        SetDevice(DeviceType.KeyboardMouse);
    }

    private void OnGamepadInput(InputAction.CallbackContext context)
    {
        // Detectar qué tipo de gamepad generó el input usando el nombre del tipo
        var device = context.control.device;
        string deviceTypeName = device.GetType().Name;
        
        switch (deviceTypeName)
        {
            case "XInputController":
            case "XInputControllerWindows":
                SetDevice(DeviceType.Xbox);
                break;
            
            case "DualShockGamepad":
            case "DualShock4GamepadHID":
            case "DualSenseGamepadHID":
            case "DualShock3GamepadHID":
                SetDevice(DeviceType.PlayStation);
                break;
            
            default:
                // Para Switch y otros gamepads genéricos
                if (IsSwitchController(device))
                {
                    SetDevice(DeviceType.Switch);
                }
                else
                {
                    SetDevice(DeviceType.GenericGamepad);
                }
                break;
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change is InputDeviceChange.Added or InputDeviceChange.Removed or 
            InputDeviceChange.Reconnected or InputDeviceChange.Disconnected)
        {
            // Debug.Log($"Device {change}: {device.displayName}");
            DetectCurrentDevice();
        }
    }

    private void DetectCurrentDevice()
    {
        // Solo detectar en el inicio si no hay dispositivo detectado
        if (_currentDevice != DeviceType.Unknown) return;

        if (Gamepad.current != null)
        {
            var pad = Gamepad.current;
            string deviceTypeName = pad.GetType().Name;
            
            switch (deviceTypeName)
            {
                case "XInputController":
                case "XInputControllerWindows":
                    SetDevice(DeviceType.Xbox);
                    break;
                
                case "DualShockGamepad":
                case "DualShock4GamepadHID":
                case "DualSenseGamepadHID":
                case "DualShock3GamepadHID":
                    SetDevice(DeviceType.PlayStation);
                    break;
                
                default:
                    SetDevice(IsSwitchController(pad) ? DeviceType.Switch : DeviceType.GenericGamepad);
                    break;
            }
        }
        else if (Keyboard.current != null || Mouse.current != null)
        {
            SetDevice(DeviceType.KeyboardMouse);
        }
    }

    private bool IsSwitchController(InputDevice device)
    {
        // Detectar Switch Pro Controller por nombre ya que la clase específica no está disponible
        var deviceName = device.displayName.ToLower();
        return deviceName.Contains("switch") || 
               deviceName.Contains("pro controller") ||
               device.description.product.ToLower().Contains("switch");
    }

    public void ForceRefresh() => DetectCurrentDevice();

    private void SetDevice(DeviceType newDevice)
    {
        if (_currentDevice == newDevice) return;
        
        _currentDevice = newDevice;
        // Debug.Log($"{GetColor()}🎮 Input device changed to: {_currentDevice}</color>");
        OnDeviceChanged?.Invoke(_currentDevice);
    }

    private string GetColor()
    {
        return _currentDevice switch
        {
            DeviceType.KeyboardMouse => "<color=yellow>",
            DeviceType.Xbox => "<color=green>",
            DeviceType.PlayStation => "<color=blue>",
            DeviceType.Switch => "<color=red>",
            DeviceType.GenericGamepad => "<color=white>",
            _ => "<color=grey>"
        };
    }

    // Cleanup cuando el ScriptableObject se destruye
    private void OnDisable()
    {
        Dispose();
    }
}