using UnityEngine;
using UnityEngine.UI;

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

    // --- Shader control centralizado ---
    public void StartDisintegrate(Shader shader, Color startColor, Vector2 minMax, float alpha = 1f)
    {
        var mat = _renderer.material;
        mat.shader = shader;
        mat.SetFloat("_ColorController", 1);
        mat.SetColor("_StartingColor", startColor);
        mat.SetVector("_MinMaxPos", minMax);
        mat.SetFloat("_Alpha", alpha);
        _outline.enabled = false;
    }

    public void SetDisintegrateAlpha(float alpha)
    {
        _renderer.material.SetFloat("_Alpha", alpha);
    }

    public void StopDisintegrate(Shader originalShader)
    {
        var mat = _renderer.material;
        mat.shader = originalShader;
        _outline.enabled = true;
    }

    public void UpdateNodeType(NodeType nodeType, Color currentOutline)
    {
        _renderer.material.SetColor("_EmissiveColor", currentOutline);
        _outline.OutlineColor = currentOutline;
    }
}
