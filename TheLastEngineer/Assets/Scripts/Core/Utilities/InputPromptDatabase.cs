using System;
using System.Collections.Generic;
using UnityEngine;
using DeviceType = InputDeviceDetector.DeviceType;

[CreateAssetMenu(fileName = "InputPromptDatabase", menuName = "Systems/Input Prompt Database")]
public class InputPromptDatabase : ScriptableObject
{
    // Acciones del juego que pueden mostrarse como prompt en la UI.
    // Mapean contra las InputAction reales de PlayerInputs (ver InputManager).
    public enum PromptAction
    {
        Move,
        Rotate,
        Interact,
        Dash,
        Pause,
        ResetCam,
        CameraLeft,
        CameraRight,
        Cancel
    }

    [Serializable]
    public class PromptEntry
    {
        public PromptAction action;
        public Sprite keyboardMouse;
        public Sprite xbox;
        public Sprite playStation;
        public Sprite nintendoSwitch;
        public Sprite genericGamepad;
    }

    [SerializeField] private List<PromptEntry> _entries = new List<PromptEntry>();

    /// <summary>
    /// Devuelve el sprite para una accion segun el dispositivo actual.
    /// Aplica fallback: si el sprite del device pedido es null, cae a genericGamepad
    /// (para cualquier gamepad) y luego a keyboardMouse. Unknown se trata como teclado.
    /// </summary>
    public Sprite GetSprite(PromptAction action, DeviceType device)
    {
        PromptEntry entry = _entries.Find(e => e.action == action);
        if (entry == null) return null;

        switch (device)
        {
            case DeviceType.KeyboardMouse:
            case DeviceType.Unknown:
                return entry.keyboardMouse;

            case DeviceType.Xbox:
                return entry.xbox != null ? entry.xbox : entry.genericGamepad;

            case DeviceType.PlayStation:
                return entry.playStation != null ? entry.playStation : entry.genericGamepad;

            case DeviceType.Switch:
                return entry.nintendoSwitch != null ? entry.nintendoSwitch : entry.genericGamepad;

            case DeviceType.GenericGamepad:
            default:
                return entry.genericGamepad;
        }
    }
}
