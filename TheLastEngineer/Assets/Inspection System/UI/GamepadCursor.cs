using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;

public class GamepadCursor : MonoBehaviour
{
    public static GamepadCursor Instance;

    [SerializeField] private RectTransform _cursorTransform;
    [SerializeField] private RectTransform _canvasTransform;
    [SerializeField] private Canvas _canvas;
    
    [SerializeField] private float _cursorSpeed = 1000f;
    [SerializeField] private float _cursorPadding = 35f;

    private PlayerInput _playerInput = default;
    private Mouse _virtualMouse = default;
    private Mouse _currentMouse = default;
    private Camera _mainCamera = default;

    private bool _prevMouseState = default;

    private string _prevControlScheme = "";
    public const string GamepadScheme = "Gamepad";
    public const string MouseScheme = "Keyboard&Mouse";

    public string PrevControlScheme { get { return _prevControlScheme; } }
    public Mouse CurrentMouse { get { return _currentMouse; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _playerInput = GetComponent<PlayerInput>();
        _mainCamera = Camera.main;
        _currentMouse = Mouse.current;
        _prevControlScheme = _playerInput.currentControlScheme;
    }

    private void Start()
    {
        InputManager.Instance.SetControlScheme(_prevControlScheme);
    }

    private void OnEnable()
    {
        if (_virtualMouse == null)
            _virtualMouse = (Mouse) InputSystem.AddDevice("VirtualMouse");
        else if (!_virtualMouse.added)
            InputSystem.AddDevice(_virtualMouse);

        if (_cursorTransform != null)
        {
            Vector2 pos = _cursorTransform.anchoredPosition;
            InputState.Change(_virtualMouse.position, pos);
        }

        InputUser.PerformPairingWithDevice(_virtualMouse, _playerInput.user);
        InputSystem.onAfterUpdate += UpdateMotion;
        _playerInput.onControlsChanged += OnControlsChanged;
    }

    private void OnDisable()
    {
        if (_virtualMouse != null && _virtualMouse.added)
        {
            _playerInput.user.UnpairDevice(_virtualMouse);
            InputSystem.RemoveDevice(_virtualMouse);
        }

        InputSystem.onAfterUpdate -= UpdateMotion;
        _playerInput.onControlsChanged -= OnControlsChanged;
    }

    private void UpdateMotion()
    {
        if (_virtualMouse == null || Gamepad.current == null) return;

        UpdatePosition();
        UpdateState();
    }

    private void UpdatePosition()
    {
        Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();

        deltaValue *= _cursorSpeed * Time.unscaledDeltaTime;

        Vector2 currentPosition = _virtualMouse.position.ReadValue();
        Vector2 newPos = currentPosition + deltaValue;

        newPos.x = Mathf.Clamp(newPos.x, _cursorPadding, Screen.width - _cursorPadding);
        newPos.y = Mathf.Clamp(newPos.y, _cursorPadding, Screen.height - _cursorPadding);

        InputState.Change(_virtualMouse.position, newPos);
        InputState.Change(_virtualMouse.delta, deltaValue);

        AnchorCursor(newPos);
    }

    private void UpdateState()
    {
        bool southButtonPressed = Gamepad.current.buttonSouth.IsPressed();

        if (_prevMouseState != southButtonPressed)
        {
            _virtualMouse.CopyState<MouseState>(out var mouseState);

            mouseState.WithButton(MouseButton.Left, southButtonPressed);
            InputState.Change(_virtualMouse, mouseState);
            _prevMouseState = southButtonPressed;
        }
    }

    private void AnchorCursor(Vector2 targetPos)
    {
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, targetPos, _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _mainCamera, out anchoredPos);

        _cursorTransform.anchoredPosition = anchoredPos;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        if (_playerInput.currentControlScheme == MouseScheme && _prevControlScheme != MouseScheme)
        {
            _cursorTransform.gameObject.SetActive(false);
            Cursor.visible = true;
            _currentMouse.WarpCursorPosition(_virtualMouse.position.ReadValue());
            _prevControlScheme = MouseScheme;
        }
        else if (_playerInput.currentControlScheme == GamepadScheme && _prevControlScheme != GamepadScheme)
        {
            _cursorTransform.gameObject.SetActive(true);
            Cursor.visible = false;
            InputState.Change(_virtualMouse.position, _currentMouse.position.ReadValue());
            AnchorCursor(_currentMouse.position.ReadValue());
            _prevControlScheme = GamepadScheme;
        }
        
        InputManager.Instance.SetControlScheme(_prevControlScheme);
    }

    public bool IsUsingGamepad()
    {
        return _prevControlScheme == GamepadScheme;
    }

    public Vector2 GetCursorPosition()
    {
        if (_prevControlScheme == GamepadScheme && _virtualMouse != null)
        {
            return _virtualMouse.position.ReadValue();
        }
        else if (_prevControlScheme == MouseScheme && _currentMouse != null)
        {
            return Input.mousePosition;
        }
        return Vector2.zero;
    }

    public void CenterCursor()
    {
        Vector2 centerPos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (_virtualMouse != null && _virtualMouse.added)
            InputState.Change(_virtualMouse.position, centerPos);

        if (_currentMouse != null)
            _currentMouse.WarpCursorPosition(centerPos);

        if (_cursorTransform != null)
            AnchorCursor(centerPos);
    }
}
