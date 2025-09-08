using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerNodeHandler : MonoBehaviour
{
    public static PlayerNodeHandler Instance = null;

    [SerializeField] private Transform _attachPos;

    public event Action<bool, NodeType> OnNodeGrabbed;
    public event Action<bool> OnAbsorbCorruption;
    public NodeController CurrentNode => _node;
    public NodeType CurrentType { get; private set; } = NodeType.None;
    public bool HasNode => _node != null;
    public bool IsCorrupted { get; private set; }
    public Vector3 AttachPos { get; private set; }

    private PlayerView _view;
    private NodeController _node;
    private Coroutine _corruptionRoutine;
    private Vector3 _absorbedPos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Pick(NodeController node)
    {
        if (_node != null || node == null) return;

        _node = node;
        CurrentType = node.NodeType;
        AttachPos = _attachPos.localPosition;

        _view = PlayerController.Instance.View;

        _view.GrabNode(true, node.CurrentColor);
        _view.PlayNodePS(node.NodeType);
        _node.OnUpdatedNodeType += OnNodeTypeUpdated;

        OnNodeGrabbed?.Invoke(true, CurrentType);
    }

    public void Release(bool isDropping = false)
    {
        if (_node == null) return;

        _node.OnUpdatedNodeType -= OnNodeTypeUpdated;

        if (isDropping)
            _node.Attach(_node.transform.position);

        ResetNode();
    }

    private void ResetNode()
    {
        _node = null;
        CurrentType = NodeType.None;
        _view.GrabNode(false, Color.black);
        OnNodeGrabbed?.Invoke(false, CurrentType);
    }

    private void OnNodeTypeUpdated(NodeType type)
    {
        CurrentType = type;

        if (_corruptionRoutine != null && CurrentType != NodeType.Corrupted)
        {
            StopCoroutine(_corruptionRoutine);
            _corruptionRoutine = null;
            OnAbsorbCorruption?.Invoke(false);
            _view.UpdatePlayerMaterials(false);
            IsCorrupted = false;
        }

        _view.GrabNode(true, _node.CurrentColor);
        _view.PlayNodePS(CurrentType);
        OnNodeGrabbed?.Invoke(true, CurrentType);
    }

    public void BeginCorruption(Transform playerTransform, Action<Vector3> setPlayerPos)
    {
        if (_corruptionRoutine != null) return;
        _corruptionRoutine = StartCoroutine(StartCorruption(playerTransform, setPlayerPos));
    }

    private IEnumerator StartCorruption(Transform tr, Action<Vector3> setPlayerPos)
    {
        OnAbsorbCorruption?.Invoke(true);
        IsCorrupted = true;
        _absorbedPos = tr.position;
        _view.UpdatePlayerMaterials(true);

        yield return new WaitForSeconds(5f);

        OnAbsorbCorruption?.Invoke(false);
        _view.UpdatePlayerMaterials(false);
        setPlayerPos?.Invoke(_absorbedPos);
        IsCorrupted = false;
        _corruptionRoutine = null;
    }
}
