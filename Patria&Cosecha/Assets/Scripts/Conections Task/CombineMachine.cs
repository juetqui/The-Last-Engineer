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
        if (_firstNode != null && _secondNode != null && IsValidCombination(_firstNode.NodeType, _secondNode.NodeType)) Activate();
        else Deactivate();

        if (_isCombining)
        {
            _firstNode.Combine(Time.deltaTime);
            _secondNode.Combine(Time.deltaTime);
        }
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
        if (!IsValidCombination(_firstNode.NodeType, _secondNode.NodeType)) return;

        _isCombining = true;

        StartCoroutine(DestroyNodes());

        _combinedNode = Instantiate(_combinedPrefab).GetComponent<ElectricityNode>();

        Vector3 newScale = new Vector3(2, 2, 2);
        _combinedNode.Attach(_combinedNodePos.position, this, newScale);
    }

    private bool IsValidCombination(NodeType firstType, NodeType secondType)
    {
        bool firstIsValid = false;
        bool secondIsValid = false;

        if (firstType == NodeType.Blue || firstType == NodeType.Purple) firstIsValid = true;
        if (secondType == NodeType.Blue || secondType == NodeType.Purple) secondIsValid = true;

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

    private IEnumerator DestroyNodes()
    {
        yield return new WaitForSeconds(2f);

        Destroy(_firstNode.gameObject);
        Destroy(_secondNode.gameObject);

        _firstNode = null;
        _secondNode = null;

        _isCombining = false;
    }
}
