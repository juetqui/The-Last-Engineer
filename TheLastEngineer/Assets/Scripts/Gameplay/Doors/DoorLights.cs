using UnityEngine;

public class DoorLights : MonoBehaviour
{
    [SerializeField] private TaskManager _tm;
    [SerializeField] private Light[] _lights;
    [SerializeField] private Color _offColor, _onColor;

    void Awake()
    {
        if (_tm == null) _tm = FindObjectOfType<TaskManager>();
        if (_tm != null)
        {
            _tm.RunningChanged += ChangeLightsColor;  // o _tm.onRunning += ...
            // _tm.onRunning += ChangeLightsColor;
        }
    }

    void Start()
    {
        ChangeLightsColor(false);
    }

    void OnDestroy()
    {
        if (_tm != null)
        {
            _tm.RunningChanged -= ChangeLightsColor;
            // _tm.onRunning -= ChangeLightsColor;
        }
    }

    private void ChangeLightsColor(bool isRunning)
    {
        var color = isRunning ? _onColor : _offColor;
        foreach (var l in _lights) if (l) l.color = color;
    }
}
