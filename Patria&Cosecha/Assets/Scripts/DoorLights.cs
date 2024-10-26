using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLights : MonoBehaviour
{
    [SerializeField] private Light[] _lights;
    [SerializeField] private TaskManager _taskManager;

    void Start()
    {
        
    }

    void Update()
    {
        
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
