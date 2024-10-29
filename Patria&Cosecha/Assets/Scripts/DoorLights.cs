using UnityEngine;

public class DoorLights : MonoBehaviour
{
    [SerializeField] private Light[] _lights;
    [SerializeField] private Color _offColor, _onColor;
    
    private TaskManager _taskManager = default;

    void Start()
    {
        ChangeLightsColor(_offColor);
    }

    void Update()
    {
        if (_taskManager.Running) ChangeLightsColor(_onColor);
    }

    private void ChangeLightsColor(Color color)
    {
        foreach (var light in _lights)
            light.color = color;
    }

    public void SetTM(MainTM mainTM)
    {
        _taskManager = mainTM;
    }

    private void CheckLights()
    {
        //for (int i = 0; i < _taskManager.connections.Count; i++)
        //{
        //    if (_taskManager.connections[i].IsWorking && _lights[i]) _lights[i].color = Color.green;
        //    else _lights[i].color = Color.red;
        //}
    }
}
