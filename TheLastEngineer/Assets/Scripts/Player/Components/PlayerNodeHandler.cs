using System;
using System.Collections;
using UnityEngine;

public class PlayerNodeHandler : MonoBehaviour
{
    public static PlayerNodeHandler Instance = null;

    [SerializeField] private Transform _attachPos;
    [SerializeField] private Transform _dropPos;

    private PlayerView _view;
    private NodeController _node;
    private Coroutine _corruptionRoutine;
    private Vector3 _absorbedPos;

    #region GETTERS
    public NodeController CurrentNode => _node;
    public GlitchState CurrentLevel { get; private set; } = GlitchState.Clean;
    public bool HasNode => _node != null;
    public bool IsCorrupted { get; private set; }
    public Transform AttachTransform { get; private set; }
    public Vector3 AttachPos { get; private set; }
    #endregion

    public Action<bool, GlitchState> OnNodeGrabbed;
    public Action<bool> OnAbsorbCorruption;
    public Action<Glitcheable> OnGlitchChange;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        AttachTransform = _attachPos;
        AttachPos = _attachPos.localPosition;
    }

    public void Pick(NodeController node)
    {
        if (_node != null || node == null) return;

        _node = node;
        CurrentLevel = node.Level;

        _view = PlayerController.Instance.View;

        _view.GrabNode(true, node.CurrentColor);
        _view.PlayNodePS(node.Level);
        _node.OnUpdatedNodeType += OnNodeTypeUpdated;

        OnNodeGrabbed?.Invoke(true, CurrentLevel);
    }

    public void Release(bool isDropping = false)
    {
        if (_node == null) return;

        _node.OnUpdatedNodeType -= OnNodeTypeUpdated;

        if (isDropping)
            _node.Attach(_dropPos.position);

        ResetNode();
    }

    private void ResetNode()
    {
        _node = null;
        CurrentLevel = GlitchState.Clean;
        _view.GrabNode(false, Color.black);
        OnNodeGrabbed?.Invoke(false, CurrentLevel);
    }

    private void OnNodeTypeUpdated(GlitchState type)
    {
        CurrentLevel = type;

        if (_corruptionRoutine != null && CurrentLevel != GlitchState.Glitched)
        {
            StopCoroutine(_corruptionRoutine);
            _corruptionRoutine = null;
            OnAbsorbCorruption?.Invoke(false);
            _view.UpdatePlayerMaterials(false);
            IsCorrupted = false;
        }

        _view.GrabNode(true, _node.CurrentColor);
        _view.PlayNodePS(CurrentLevel);
        OnNodeGrabbed?.Invoke(true, CurrentLevel);
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
