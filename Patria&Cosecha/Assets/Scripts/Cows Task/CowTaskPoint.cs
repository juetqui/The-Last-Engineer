using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CowTaskPoint : MonoBehaviour
{
    public PlayerTDController player = default;
    public CowTaskPointsGenerator taskPointGenerator = default;
    public CowsTaskManager taskManager = default;

    private float _catchPosition = 3f;
    private bool _canAddToCounter = true;

    private void Update()
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= _catchPosition && _canAddToCounter)
            AddPointsToCounter();
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        _canAddToCounter = true;
    }

    private void AddPointsToCounter()
    {
        _canAddToCounter = false;
        taskManager.AddToCounter();
        taskPointGenerator.RespawnPoint();
    }
}
