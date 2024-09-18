using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowTaskPointsGenerator : MonoBehaviour
{
    private PlayerTDController _player = default;
    
    [SerializeField] private CowsTaskManager _taskManager = default;
    [SerializeField] private CowTaskPoint _pointPrefab = default;
    
    private CowTaskPoint _currentInstance = default;
    
    [SerializeField] private float _generateDistance = 12f, _thresholdDistance = 2f;

    private bool _isNewPoint = true, _respawnPoint = false;
    private Vector3 _lastSpawnPosition = Vector3.zero;

    private void Start()
    {
        _pointPrefab.player = _player = _taskManager.Player;
        _pointPrefab.taskManager = _taskManager;
        _pointPrefab.taskPointGenerator = this;
    }

    private void Update()
    {
        if (_taskManager.TaskStarted && _isNewPoint)
        {
            _isNewPoint = false;
            _currentInstance = Instantiate(_pointPrefab);
            SpawnPoint();
        }
        else if (_taskManager.TaskStarted && _respawnPoint) SpawnPoint();
        else if (!_taskManager.TaskStarted && _currentInstance != null)
        {
            _isNewPoint = true;
            Destroy(_currentInstance.gameObject);
        }
    }

    private void SpawnPoint()
    {
        _currentInstance.UpdatePosition(GenerateRandomPosition());
        _respawnPoint = false;
    }

    public void RespawnPoint()
    {
        _respawnPoint = true;
    }

    private Vector3 GenerateRandomPosition()
    {
        float randomX = Random.Range(-_generateDistance, _generateDistance + 1);
        float randomZ = Random.Range(-_generateDistance, _generateDistance + 1);

        Vector3 newPosition = new Vector3(transform.position.x + randomX, 1f, transform.position.z + randomZ);

        if (Vector3.Distance(_lastSpawnPosition, newPosition) <= _thresholdDistance) GenerateRandomPosition();
        
        _lastSpawnPosition = newPosition;

        return newPosition;
    }
}
