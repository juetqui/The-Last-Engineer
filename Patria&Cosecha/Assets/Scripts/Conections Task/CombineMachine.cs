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
    public bool HasNodes { get { return IsFilled(); } }

    private void Update()
    {
        if (IsFilled() && IsValidCombination()) Activate();
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
        if (!IsValidCombination() || !IsFilled()) return;

        Vector3 newScale = new Vector3(1.5f, 1.5f, 1.5f);

        _isCombining = true;
        _combinedNode = Instantiate(_combinedPrefab, transform.position, Quaternion.identity).GetComponent<ElectricityNode>();
        _combinedNode.Attach(_combinedNodePos.localPosition, transform, newScale);
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

    private bool IsValidCombination()
    {
        bool firstIsValid = false;
        bool secondIsValid = false;

        if (_firstNode.NodeType == NodeType.Blue || _firstNode.NodeType == NodeType.Purple) firstIsValid = true;
        if (_secondNode.NodeType == NodeType.Blue || _secondNode.NodeType == NodeType.Purple) secondIsValid = true;

        if (firstIsValid && secondIsValid && _firstNode != _secondNode) return true;
        else return false;
    }

    private bool IsFilled()
    {
        if (_firstNode == null || _secondNode == null) return false;
        
        return true;
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
