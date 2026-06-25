using UnityEngine;
using PrimeTween;

public class ConnectionLightView : MonoBehaviour
{
    [SerializeField] private Ease _tweenType = Ease.InOutSine;

    [SerializeField] private Color _lightDefault;
    [SerializeField] private Color _lightCorrupted;
    [SerializeField] private Color _lightOff;

    private Light _light = default;
    private Connection _connection = default;
    private Color _lightOn = default;

    void Start()
    {
        _light = GetComponent<Light>();
        _connection = GetComponentInParent<Connection>();

        _lightOn = _connection.RequiredType == NodeType.Default ? _lightDefault : _lightCorrupted;

        TurnOn();
    }

    private void TurnOn()
    {
        Color currentColor = _light.color;

        Tween.Custom(gameObject, currentColor, _lightOn, 0.6f, (_, c) => UpdateColor(c), _tweenType)
            .OnComplete(turnOff);
    }

    private void turnOff()
    {
        Color currentColor = _light.color;

        Tween.Custom(gameObject, currentColor, _lightOff, 0.6f, (_, c) => UpdateColor(c), _tweenType)
            .OnComplete(TurnOn);
    }

    private void UpdateColor(Color c)
    {
        _light.color = c;
    }
}
