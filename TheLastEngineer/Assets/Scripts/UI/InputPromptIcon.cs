using UnityEngine;
using UnityEngine.UI;
using DeviceType = InputDeviceDetector.DeviceType;

/// <summary>
/// Coloca este componente en una Image de UI para que su sprite cambie
/// automaticamente segun el dispositivo de input activo (teclado, Xbox, PS, Switch,
/// gamepad generico). Sirve para prompts de tutorial, HUD y menus por igual.
/// </summary>
[RequireComponent(typeof(Image))]
public class InputPromptIcon : MonoBehaviour
{
    [SerializeField] private InputDeviceDetector _detector = default;
    [SerializeField] private InputPromptDatabase _database = default;
    [SerializeField] private InputPromptDatabase.PromptAction _action = default;

    [Tooltip("Si el sprite resuelto es null, oculta la Image en lugar de dejarla vacia.")]
    [SerializeField] private bool _hideWhenNoSprite = true;

    private Image _image = default;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_detector == null || _database == null) return;

        _detector.OnDeviceChanged += Refresh;
        // Refrescar de inmediato por si el device ya cambio antes de habilitarse.
        Refresh(_detector.CurrentDevice);
    }

    private void OnDisable()
    {
        if (_detector != null)
            _detector.OnDeviceChanged -= Refresh;
    }

    private void Refresh(DeviceType device)
    {
        if (_image == null || _database == null) return;

        Sprite sprite = _database.GetSprite(_action, device);
        _image.sprite = sprite;

        if (_hideWhenNoSprite)
            _image.enabled = sprite != null;
    }
}
