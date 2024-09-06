using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CowTaskPoint : MonoBehaviour
{
    public PlayerController player = default;
    public CowTaskPointsGenerator taskPointGenerator = default;
    public CowsTaskManager taskManager = default;

    private float _catchPosition = 3f;
    private bool _canAddToCounter = true;

    private void Update()
    {
        Debug.Log("Position: " + transform.position);
        if (Vector3.Distance(transform.position, player.transform.position) <= _catchPosition && _canAddToCounter)
            AddPointsToCounter();
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        Debug.Log("Spawn Point: " + newPosition);
        transform.position = newPosition;
        _canAddToCounter = true;
    }

    private void AddPointsToCounter()
    {
        taskManager.AddToCounter();
        taskPointGenerator.RespawnPoint();
        _canAddToCounter = false;
    }
}
