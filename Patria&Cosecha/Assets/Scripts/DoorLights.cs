using UnityEngine;

public class DoorLights : MonoBehaviour
{
    [SerializeField] private Light[] _lights;
    [SerializeField] private Color _offColor, _onColor;

    void Start()
    {
        MainTM.Instance.onRunning += ChangeLightsColor;
        ChangeLightsColor(false);
    }

    private void ChangeLightsColor(bool isRunning)
    {
        if (isRunning)
        {
            foreach (var light in _lights)
                light.color = _onColor;
        }
        else
        {
            foreach (var light in _lights)
                light.color = _offColor;
        }
    }
}
