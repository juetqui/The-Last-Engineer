using System.Collections.Generic;
using UnityEngine;

public class NodeEffectController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredType;
    [SerializeField] private List<Transform> _objectsToMove;
    [SerializeField] private List<Transform> _newPositions;

    private List<Vector3> _originalPositions = new List<Vector3>();
    private List<Quaternion> _originalRotations = new List<Quaternion>();

    private void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += ToggleObjects;

        _originalPositions.Clear();
        _originalRotations.Clear();

        foreach (var obj in _objectsToMove)
        {
            _originalPositions.Add(obj.position);
            _originalRotations.Add(obj.rotation);
        }
    }

    private void ToggleObjects(bool toggle, NodeType nodeType)
    {
        if (nodeType != _requiredType) return;

        if (toggle)
        {
            MoveObjects();
        }
        else
        {
            ResetObjects();
        }
    }

    private void MoveObjects()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            if (i < _newPositions.Count && _newPositions[i] != null)
            {
                _objectsToMove[i].position = _newPositions[i].position;
                _objectsToMove[i].rotation = _newPositions[i].rotation;
            }
        }
    }

    private void ResetObjects()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            _objectsToMove[i].position = _originalPositions[i];
            _objectsToMove[i].rotation = _originalRotations[i];
        }
    }
}
