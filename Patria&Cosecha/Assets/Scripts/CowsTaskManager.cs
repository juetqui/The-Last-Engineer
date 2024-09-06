using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowsTaskManager : MonoBehaviour
{
    
    [SerializeField]private PlayerController _player = default;
    [SerializeField] private GameObject _fenceDoor = default;
    [SerializeField] private Transform _startPoint = default;
    [SerializeField] private Material _taskMaterial = default;
    [SerializeField] private float _interactionDistance = default;

    private bool _taskStarted = false, _isTaskDone = default;
    private int _taskCount = 0, _totalToFinish = 5;
    
    public PlayerController Player { get => _player; }
    public bool TaskStarted { get => _taskStarted; }

    private void Start()
    {
        _isTaskDone = false;
    }

    private void Update()
    {
        if (!_taskStarted || _isTaskDone) CheckInitTask();

        if (_isTaskDone) _fenceDoor.SetActive(false);
    }

    private void CheckInitTask()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) < _interactionDistance)
        {
            _taskMaterial.color = Color.green;

            if (Input.GetKeyDown(KeyCode.E)) StartTask();
        }
        else _taskMaterial.color = Color.yellow;
    }

    private void StartTask()
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

    public void FinishTask() => _isTaskDone = true;
}
