using UnityEngine;

public class SecDoorLights : MonoBehaviour
{
    [SerializeField] SecondaryTM _secTM;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private float _lerpTime;

    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _closedColor;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _openedColor;

    private Color _currentColor = default, _startColor = default, _targetColor = default;
    private float _time = 0;
    private bool _isLerping = false, _isRunning = false;

    void Start()
    {
        _renderer.material.SetColor("_EmissionColor", _closedColor);
        _secTM.onRunning += StartLerp;
    }

    void Update()
    {
        if (_isLerping) LerpColors();
    }

    private void StartLerp(bool isRunning)
    {
        _startColor = isRunning ? _closedColor : _openedColor;
        _targetColor = isRunning ? _openedColor : _closedColor;
        _time = 0;
        _isLerping = true;
    }

    private void LerpColors()
    {
        _time += Time.deltaTime / _lerpTime;
        _time = Mathf.Clamp01(_time);
        _currentColor = Color.Lerp(_startColor, _targetColor, _time);
        _renderer.material.SetColor("_EmissionColor", _currentColor);

        if (_time >= 1.0f) _isLerping = false;
    }
}
