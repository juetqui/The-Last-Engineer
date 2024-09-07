using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowsTaskManager : MonoBehaviour
{
    
    [SerializeField] private PlayerController _player = default;
    [SerializeField] private GameObject _fenceDoor = default;
    [SerializeField] private Transform _startPoint = default;
    [SerializeField] private Material _taskMaterial = default;
    
    private bool _taskStarted = false;
    private float _interactionDistance = default;
    private int _taskCount = 0, _totalToFinish = 5;
    
    public PlayerController Player { get { return _player; } }
    public bool TaskStarted { get { return _taskStarted; } }

    private void Start()
    {
        _taskStarted = false;
        _interactionDistance = _player.TaskInteractionDistance;
    }

    private void Update()
    {
        if (!_taskStarted) CheckInitTask();
    }

    private void CheckInitTask()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) < _interactionDistance) _taskMaterial.color = Color.green;
        else _taskMaterial.color = Color.yellow;
    }

    public void StartTask()
    {
        _taskStarted = true;
        _player.transform.position = _startPoint.position;
        _fenceDoor.SetActive(true);
    }

    public void AddToCounter()
    {
        _taskCount++;
        if (_taskCount >= _totalToFinish) FinishTask();
    }

    public void FinishTask()
    {
        _taskStarted = false;
        _fenceDoor.SetActive(false);
    }
}
