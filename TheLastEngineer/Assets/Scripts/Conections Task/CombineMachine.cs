using UnityEngine;

public class CombineMachine : MonoBehaviour, IInteractable, IConnectable
{
    [SerializeField] private GameObject _combinedPrefab;
    [SerializeField] private Collider _trigger;
    [SerializeField] private GameObject _firstNodePos;
    [SerializeField] private GameObject _secondNodePos;
    [SerializeField] private Transform _combinedNodePos;

    public InteractablePriority Priority => InteractablePriority.Medium;
    public Transform Transform => transform;

    private NodeController _firstNode = default, _secondNode = default, _combinedNode = default;
    private bool _isActive = false, _isCombining = false;

    public bool IsActive { get { return _isActive; } }

    private void Update()
    {
        if (IsFilled() && IsValidCombination()) Activate();
        else Deactivate();

        if (_isCombining) DestroyNodes(Time.deltaTime);
    }

    public bool CanInteract(PlayerTDController player)
    {
        return player.HasNode() && !_isActive && !IsFilled();
    }

    public void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (CanInteract(player) && player.GetCurrentNode() != null)
        {
            NodeController node = player.GetCurrentNode();
            SetNode(node);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
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
        if (!IsValidCombination() || !IsFilled()) return;

        Vector3 newScale = new Vector3(1.5f, 1.5f, 1.5f);

        _isCombining = true;
        _combinedNode = Instantiate(_combinedPrefab, transform.position, Quaternion.identity).GetComponent<NodeController>();
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
        return _firstNode != null && _secondNode != null;
    }

    private void SetNode(NodeController node)
    {
        if (node == null || node.NodeType == NodeType.Dash) return;

        if (_firstNode == null)
        {
            node.Attach(_firstNodePos.transform.localPosition, transform);
            _firstNodePos.SetActive(false);
            _firstNode = node;
        }
        else if (_firstNode != null && node.NodeType != _firstNode.NodeType)
        {
            node.Attach(_secondNodePos.transform.localPosition, transform);
            _secondNodePos.SetActive(false);
            _secondNode = node;
        }
    }

    public void UnsetNode(NodeController node)
    {
        if (node.NodeType == NodeType.Dash) return;

        if (_firstNode != null || _secondNode != null)
        {
            if (node.NodeType == _firstNode.NodeType)
            {
                _firstNode = null;
                _firstNodePos.SetActive(true);
            }
            else if (node.NodeType == _secondNode.NodeType)
            {
                _secondNode = null;
                _secondNodePos.SetActive(true);
            }
        }
    }
}
