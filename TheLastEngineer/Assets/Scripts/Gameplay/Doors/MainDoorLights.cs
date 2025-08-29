using UnityEngine;

public class MainDoorLights : MonoBehaviour
{
    [SerializeField] private TaskManager _tm;
    [SerializeField] private float _lerpTime = 0.5f;

    [ColorUsage(true, true)][SerializeField] private Color _closedColor;
    [ColorUsage(true, true)][SerializeField] private Color _openedColor;

    private MeshRenderer _renderer;
    private Color _startColor, _targetColor;
    private float _t;
    private bool _isLerping;
    private bool _firstEventSkipped; // para replicar tu _index

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (_tm == null) _tm = FindObjectOfType<TaskManager>();
        if (_tm != null)
        {
            _tm.RunningChanged += StartLerp;
            // _tm.onRunning += StartLerp;
        }
    }

    void Start()
    {
        SetEmission(_closedColor);
    }

    void OnDestroy()
    {
        if (_tm != null)
        {
            _tm.RunningChanged -= StartLerp;
            // _tm.onRunning -= StartLerp;
        }
    }

    void Update()
    {
        if (!_isLerping) return;

        _t += Time.deltaTime / Mathf.Max(0.0001f, _lerpTime);
        var c = Color.Lerp(_startColor, _targetColor, Mathf.Clamp01(_t));
        SetEmission(c);

        if (_t >= 1f) _isLerping = false;
    }

    private void StartLerp(bool isRunning)
    {
        if (!_firstEventSkipped) { _firstEventSkipped = true; return; }

        _startColor = isRunning ? _closedColor : _openedColor;
        _targetColor = isRunning ? _openedColor : _closedColor;
        _t = 0f;
        _isLerping = true;
    }

    private void SetEmission(Color c)
    {
        if (_renderer && _renderer.material)
            _renderer.material.SetColor("_EmissionColor", c);
    }
}
