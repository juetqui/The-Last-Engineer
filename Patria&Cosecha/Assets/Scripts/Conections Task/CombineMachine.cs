using UnityEngine;

public class CombineMachine : MonoBehaviour
{
    [SerializeField] private GameObject _combinedPrefab;
    [SerializeField] private Collider _trigger;
    [SerializeField] private Transform _firstNodePos;
    [SerializeField] private Transform _secondNodePos;
    [SerializeField] private Transform _combinedNodePos;

    private ElectricityNode _firstNode = default, _secondNode = default, _combinedNode;
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

        //Destroy(_firstNode.gameObject);
        //Destroy(_secondNode.gameObject);
        GameObject combinedNode = Instantiate(_combinedPrefab, _combinedNodePos);

        _combinedNode = combinedNode.GetComponent<ElectricityNode>();

        Vector3 newScale = new Vector3(2, 2, 2);
        _combinedNode.Attach(_combinedNodePos.position, transform, newScale);
    }

    private bool IsValidCombination(NodeType firstType, NodeType secondType) => (firstType != NodeType.Dash || secondType != NodeType.Dash);

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
        if (_firstNode != null || _secondNode != null)
        {
            if (node.NodeType == _firstNode.NodeType) _firstNode = null;
            else if (node.NodeType == _secondNode.NodeType) _secondNode = null;
        }
    }
}
