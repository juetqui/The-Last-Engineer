using UnityEngine;

public class ConnectionLightListener : MonoBehaviour
{
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _enabledColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _disabledColor;

    private DoorsView _door = default;
    private Light _light = default;
    private Color _targetColor = default;
    
    private float _timer = 0f;
    private float _lerpTime = 1f;
    private bool _startLerp = false;

    void Awake()
    {
        _light = GetComponent<Light>();
        _door = GetComponentInParent<DoorsView>();
        _door.OnOpen += CheckOpenDoor;
    }

    void Update()
    {
        if (_startLerp) LerpColors();
    }

    private void CheckOpenDoor(bool isOpen)
    {
        if (isOpen)
            _targetColor = _enabledColor;
        else
            _targetColor = _disabledColor;

        _startLerp = true;
    }

    private void LerpColors()
    {
        _timer += Time.deltaTime;

        Color currentColor = _light.color;
        Color newColor = Color.Lerp(currentColor, _targetColor, _timer);

        _light.color = newColor;

        if (_timer >= _lerpTime)
        {
            _startLerp = false;
            _timer = 0f;
        }
    }
}
