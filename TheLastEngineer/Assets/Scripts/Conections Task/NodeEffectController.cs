using System.Collections.Generic;
using UnityEngine;

public class NodeEffectController : MonoBehaviour
{
    public List<Transform> objectsToMove;
    public List<Transform> newPositions;

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();

    private void Start()
    {
        originalPositions.Clear();
        originalRotations.Clear();

        foreach (var obj in objectsToMove)
        {
            originalPositions.Add(obj.position);
            originalRotations.Add(obj.rotation);
        }
    }

    public void OnNodeActivated(NodeType nodeType)
    {
        if (nodeType == NodeType.Green)
            MoveObjects();
    }

    public void OnNodeDeactivated(NodeType nodeType)
    {
        if (nodeType == NodeType.Green)
            ResetObjects();
    }

    private void MoveObjects()
    {
        for (int i = 0; i < objectsToMove.Count; i++)
        {
            if (i < newPositions.Count && newPositions[i] != null)
            {
                objectsToMove[i].position = newPositions[i].position;
                objectsToMove[i].rotation = newPositions[i].rotation;
            }
        }
    }

    private void ResetObjects()
    {
        for (int i = 0; i < objectsToMove.Count; i++)
        {
            objectsToMove[i].position = originalPositions[i];
            objectsToMove[i].rotation = originalRotations[i];
        }
    }
}
