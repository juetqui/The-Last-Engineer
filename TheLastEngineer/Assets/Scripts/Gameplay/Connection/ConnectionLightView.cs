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
    public bool _keepOn;

    private void Awake()
    {

    }
    void Start()
    {
        _light = GetComponent<Light>();
        _connection = GetComponentInParent<Connection>();
        _connection.OnNodeConnected += SetCorrectNode;

        _lightOn = _connection.RequiredType == NodeType.Default ? _lightDefault : _lightCorrupted;
        //TurnOn();
    }

    private void TurnOn()
    {
        Color currentColor = _light.color;

        var tween=Tween.Custom(gameObject, currentColor, _lightOn, 0.6f, (_, c) => UpdateColor(c), _tweenType);
        if (!_keepOn)
        {
            tween.OnComplete(turnOff);
        }
       
    }
    public void SetCorrectNode(NodeType nodeType, bool a)
    {
        if (a)
        {
            _keepOn = true;

        }
        else
        {
            _keepOn = false;

        }
        TurnOn();

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
