using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NodeEffectController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredType;
    [SerializeField] private List<Transform> _objectsToMove;
    [SerializeField] private List<Transform> _newPositions;
    [SerializeField] private float _moveSpeed;

    private List<Vector3> _originalPositions = new List<Vector3>();
    private List<Quaternion> _originalRotations = new List<Quaternion>();

    private bool _enabled = false;

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

    private void Update()
    {
        if (_enabled)
        {
            MoveObjects();
        }
        else
        {
            ResetObjects();
        }
    }

    private void ToggleObjects(bool toggle, NodeType nodeType)
    {
        if (nodeType != _requiredType) return;

        _enabled = toggle;
        
        //if (toggle)
        //{
        //    MoveObjects();
        //}
        //else
        //{
        //    ResetObjects();
        //}
    }

    private void MoveObjects()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            if (i >= _newPositions.Count || _objectsToMove[i] == null || _newPositions[i] == null)
                continue;

            float initialDistance = Vector3.Distance(_originalPositions[i], _newPositions[i].position);
            float currentDistance = Vector3.Distance(_objectsToMove[i].position, _newPositions[i].position);
            float t = initialDistance > 0.001f ? 1f - (currentDistance / initialDistance) : 1f;

            _objectsToMove[i].position = Vector3.MoveTowards(
                _objectsToMove[i].position,
                _newPositions[i].position,
                Time.deltaTime * _moveSpeed
            );

            _objectsToMove[i].rotation = Quaternion.Slerp(
                _originalRotations[i],
                _newPositions[i].rotation,
                t
            );
        }
    }

    private void ResetObjects()
    {
        for (int i = 0; i < _objectsToMove.Count; i++)
        {
            if (i >= _originalPositions.Count || _objectsToMove[i] == null || _newPositions[i] == null)
                continue;

            float initialDistance = Vector3.Distance(_newPositions[i].position, _originalPositions[i]);
            float currentDistance = Vector3.Distance(_objectsToMove[i].position, _originalPositions[i]);
            float t = initialDistance > 0.001f ? 1f - (currentDistance / initialDistance) : 1f;

            _objectsToMove[i].position = Vector3.MoveTowards(
                _objectsToMove[i].position,
                _originalPositions[i],
                Time.deltaTime * _moveSpeed
            );

            _objectsToMove[i].rotation = Quaternion.Slerp(
                _newPositions[i].rotation,
                _originalRotations[i],
                t
            );
        }
    }
}
