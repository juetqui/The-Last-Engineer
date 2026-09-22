using UnityEngine;

public class OutlineController : MonoBehaviour
{
    Outline _outline;
    NodeController _nodeController;

    [SerializeField] private Color _emissionDefault;
    [SerializeField] private Color _emissionCorrupted;

    private void Awake()
    {
        _outline = GetComponent<Outline>();   
        _nodeController = GetComponentInParent<NodeController>();
        _nodeController.OnUpdatedNodeType += ChangeOutline;
        _nodeController.OnEnableOutline += EnableOutline;
    }
    private void ChangeOutline(GlitchState node)
    {
        if (node == GlitchState.Glitched)
        {
            _outline.OutlineColor = _emissionCorrupted;
        }
        else
        {
            _outline.OutlineColor = _emissionDefault;
        }
    }

    private void EnableOutline(bool enable)
    {
        _outline.enabled = enable;
    }
}
