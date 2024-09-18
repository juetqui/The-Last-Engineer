using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowsTaskManager : MonoBehaviour
{

    [SerializeField] private PlayerTDController _player = default;
    [SerializeField] private GameObject _fenceDoor = default;
    [SerializeField] private Transform _startPoint = default;
    [SerializeField] private Material _taskMaterial = default;

    private bool _taskStarted = false;
    private float _interactionDistance = default;
    private int _tasksCompleted = 0, _taskCount = 0, _totalToFinish = 5;

    public PlayerTDController Player { get { return _player; } }
    public bool TaskStarted { get { return _taskStarted; } }
    public int CurrentPoints { get { return _taskCount; } }
    public int TotalPoints { get { return _totalToFinish; } }
    public int CompletedTasks { get { return _tasksCompleted; } }

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
        _taskCount = 0;
        _tasksCompleted++;
        _fenceDoor.SetActive(false);
    }
}
