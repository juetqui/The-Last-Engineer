using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowTaskPointsGenerator : MonoBehaviour
{
    private PlayerController _player = default;
    
    [SerializeField] private CowsTaskManager _taskManager = default;
    [SerializeField] private CowTaskPoint _pointPrefab = default;

    [SerializeField] private float _generateDistance = 12f;

    private bool _isNewPoint = true, _respawnPoint = false;

    private void Start()
    {
        _pointPrefab.player = _player = _taskManager.Player;
        _pointPrefab.taskManager = _taskManager;
        _pointPrefab.taskPointGenerator = this;
    }

    private void Update()
    {
        if(_taskManager.TaskStarted && _isNewPoint || _taskManager.TaskStarted && _respawnPoint) SpawnPoint();
    }

    private void SpawnPoint()
    {
        _pointPrefab.UpdatePosition(GenerateRandomPosition());

        if (_isNewPoint)
        {
            Instantiate(_pointPrefab);
            _isNewPoint = false;
        }
        else _respawnPoint = false;
    }

    private Vector3 GenerateRandomPosition()
    {
        float randomX = Random.Range(-_generateDistance, _generateDistance + 1);
        float randomZ = Random.Range(-_generateDistance, _generateDistance + 1);

        return new Vector3(transform.position.x + randomX, 2, transform.position.z + randomZ);
    }

    public void RespawnPoint()
    {
        _respawnPoint = true;
    }

    private void OnDrawGizmos()
    {
        Vector3 rayInit = new(transform.position.x, 2, transform.position.z);
        
        Vector3 rayDirectionX = new(_generateDistance, 0, 0);
        Vector3 rayDirectionZ = new(0, 0, _generateDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(rayInit, rayDirectionX);
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayInit, rayDirectionZ);
    }
}
