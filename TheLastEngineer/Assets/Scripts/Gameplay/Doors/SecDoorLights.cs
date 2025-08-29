using System.Collections;
using UnityEngine;

public class SecDoorLights : MonoBehaviour
{
    [SerializeField] private TaskManager _tm;
    [SerializeField] private Light _light;
    [SerializeField] private float _lerpTime = 0.5f;

    [ColorUsage(true, true)]
    [SerializeField] private Color _closedColor;

    [ColorUsage(true, true)]
    [SerializeField] private Color _openedColor;

    private MeshRenderer _renderer;
    private Color _currentColor, _startColor, _targetColor;
    private int _index = 0;
    private float _time = 0;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.material.SetColor("_EmissionColor", _closedColor);
        if (_light != null) _light.color = _closedColor;

        if (_tm == null) _tm = FindObjectOfType<TaskManager>();

        // Si tu TM tiene RunningChanged:
        _tm.RunningChanged += StartLerp;

        // Si en tu TM quedó onRunning(bool) como en los viejos scripts:
        // _tm.onRunning += StartLerp;
    }

    private void OnDestroy()
    {
        if (_tm != null)
        {
            _tm.RunningChanged -= StartLerp;
            // _tm.onRunning -= StartLerp; // alternativa si usas el viejo evento
        }
    }

    private void StartLerp(bool isRunning)
    {
        if (_index == 0) { _index++; return; }

        _startColor = isRunning ? _closedColor : _openedColor;
        _targetColor = isRunning ? _openedColor : _closedColor;

        StartCoroutine(LerpColors());
    }

    private IEnumerator LerpColors()
    {
        _time = 0f;

        while (_time < 1f)
        {
            _time += Time.deltaTime / Mathf.Max(0.0001f, _lerpTime);
            _time = Mathf.Clamp01(_time);
            _currentColor = Color.Lerp(_startColor, _targetColor, _time);
            _renderer.material.SetColor("_EmissionColor", _currentColor);

            if (_light != null)
                _light.color = _currentColor;

            yield return null;
        }
    }
}
