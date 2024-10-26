using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private GameObject _combinedPrefab;
    [SerializeField] private Collider _trigger;
    [SerializeField] private Transform _firstNodePos;
    [SerializeField] private Transform _secondNodePos;
    [SerializeField] private Transform _combinedNodePos;

    private ElectricityNode _firstNode = default, _secondNode = default, _combinedNode = default;
    private bool _isActive = false, _isCombining = false;

    public bool IsActive { get { return _isActive; } }

    private void Update()
    {
        if (_firstNode != null && _secondNode != null && IsValidCombination(_firstNode, _secondNode)) Activate();
        else Deactivate();

        if (_isCombining) DestroyNodes(Time.deltaTime);
    }

    private void Activate()
    {
        _isActive = true;
        _trigger.enabled = false;
    }

    private void Deactivate()
    {
        _isActive = false;
        _trigger.enabled = true;
    }

    public void CombineNodes()
    {
        if (!IsValidCombination(_firstNode, _secondNode)) return;

        _isCombining = true;
        _combinedNode = Instantiate(_combinedPrefab, transform.position, Quaternion.identity).GetComponent<ElectricityNode>();
        _combinedNode.Attach(_combinedNodePos.localPosition, transform);
    }

    private void DestroyNodes(float deltaTime)
    {
        if (_firstNode.Combine(deltaTime) && _secondNode.Combine(deltaTime))
        {
            Destroy(_firstNode.gameObject);
            Destroy(_secondNode.gameObject);

            _firstNode = null;
            _secondNode = null;
            
            _isCombining = false;
        }
    }

    private bool IsValidCombination(ElectricityNode firstType, ElectricityNode secondType)
    {
        if (_firstNode == null || _secondNode == null) return false;

        bool firstIsValid = false;
        bool secondIsValid = false;

        if (firstType.NodeType == NodeType.Blue || firstType.NodeType == NodeType.Purple) firstIsValid = true;
        if (secondType.NodeType == NodeType.Blue || secondType.NodeType == NodeType.Purple) secondIsValid = true;

        if (firstIsValid && secondIsValid && firstType != secondType) return true;
        else return false;
    }

    public void SetNode(ElectricityNode node)
    {
        if (node == null) return;

        if (_firstNode == null)
        {
            node.Attach(_firstNodePos.localPosition, transform);
            _firstNode = node;
        }
        else if (_firstNode != null && node.NodeType != _firstNode.NodeType)
        {
            node.Attach(_secondNodePos.localPosition, transform);
            _secondNode = node;
        }
    }

    public void UnsetNode(ElectricityNode node)
    {
        if (node.NodeType == NodeType.Dash) return;

        if (_firstNode != null || _secondNode != null)
        {
            if (node.NodeType == _firstNode.NodeType) _firstNode = null;
            else if (node.NodeType == _secondNode.NodeType) _secondNode = null;
        }
    }
}
