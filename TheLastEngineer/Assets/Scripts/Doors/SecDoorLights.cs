using System.Collections;
using UnityEngine;

public class SecDoorLights : MonoBehaviour
{
    [SerializeField] SecondaryTM _secTM;
    [SerializeField] Light _light;
    [SerializeField] private float _lerpTime;

    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _closedColor;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _openedColor;

    private MeshRenderer _renderer;
    private Color _currentColor = default, _startColor = default, _targetColor = default;
    private int _index = 0;
    private float _time = 0;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.material.SetColor("_EmissionColor", _closedColor);
        _light.color = _closedColor;
        _secTM.onRunning += StartLerp;
    }

    private void StartLerp(bool isRunning)
    {
        if (_index == 0)
        {
            _index++;
            return;
        }

        _startColor = isRunning ? _closedColor : _openedColor;
        _targetColor = isRunning ? _openedColor : _closedColor;

        StartCoroutine(LerpColors());
    }

    private IEnumerator LerpColors()
    {
        _time = 0f;

        while (_time < _lerpTime)
        {
            _time += Time.deltaTime / _lerpTime;
            _time = Mathf.Clamp01(_time);
            _currentColor = Color.Lerp(_startColor, _targetColor, _time);
            _renderer.material.SetColor("_EmissionColor", _currentColor);

            if (_light != null)
                _light.color = _currentColor;

            yield return null;
        }
    }
}
