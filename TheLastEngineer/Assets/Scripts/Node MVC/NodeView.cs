using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Collider _collider = default;
    private Outline _outline = default;
    private Color _outlineColor;
    private bool _isReseting = false;
    Animator _myAnimator;

    public bool IsReseting { get { return _isReseting; } }

    public NodeView(Renderer renderer, Collider collider, Outline outline, Color outlineColor,Animator animator)
    {
        _renderer = renderer;
        _collider = collider;
        _outline = outline;
        _outlineColor = outlineColor;
        _myAnimator = animator;
    }

    public void OnStart()
    {
        _outline.OutlineColor = _outlineColor;
    }

    public void EnableColl(bool onOff)
    {
        if (_collider.enabled == onOff) return;

        _collider.enabled = onOff;
    }

    public void SetIdleAnim()
    {
        _myAnimator.SetBool("IsOnRange", false);
        _myAnimator.SetBool("IsCollected", false);
    }
    
    public void SetCollectedAnim()
    {
        _myAnimator.SetBool("IsOnRange", false);
        _myAnimator.SetBool("IsCollected", true);
    }
    
    public void SetRangeAnim()
    {
        _myAnimator.SetBool("IsOnRange", true);
        _myAnimator.SetBool("IsCollected", false);
    }

    public void UpdateNodeType(NodeType nodeType, Color currentOutline)
    {
        if (nodeType == NodeType.Default)
        {
            Material[] mats = _renderer.sharedMaterials;
            mats[2] = Resources.Load<Material>("Materials/M_DefaultNode");
            _renderer.sharedMaterials = mats;
        }
        else
        {
            Material[] mats = _renderer.sharedMaterials;
            mats[2] = Resources.Load<Material>("Materials/M_CorruptedNode");
            _renderer.sharedMaterials = mats;
        }

        _outline.OutlineColor = currentOutline;
    }
}
