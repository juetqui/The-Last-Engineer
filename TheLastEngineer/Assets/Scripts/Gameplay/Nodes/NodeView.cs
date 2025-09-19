using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Collider _collider = default;
    private Outline _outline = default;
    private Color _outlineColor;
    private bool _isReseting = false;
    Animator _myAnimator;

    // Estado para FX
    private ParticleSystem[] _particles = System.Array.Empty<ParticleSystem>();
    private bool _isNear = false;
    private NodeType _currentType = NodeType.Default;

    public bool IsReseting { get { return _isReseting; } }

    public NodeView(Renderer renderer, Collider collider, Outline outline, Color outlineColor, Animator animator, ParticleSystem[] particles)
    {
        _renderer = renderer;
        _collider = collider;
        _outline = outline;
        _outlineColor = outlineColor;
        _myAnimator = animator;
        _particles = particles ?? System.Array.Empty<ParticleSystem>();
    }

    public void OnStart()
    {
        _outline.OutlineColor = _outlineColor;
        StopAllFX();
    }

    public void UpdateNodeType(NodeType nodeType, Color currentOutline)
    {
        _currentType = nodeType;
        _renderer.material.SetColor("_EmissiveColor", currentOutline);
        _outline.OutlineColor = currentOutline;

        RefreshFX();
    }

    public void EnableOutline(bool onOff)
    {
        if (_outline.enabled == onOff) return;

        _outline.enabled = onOff;
    }

    public void EnableColl(bool onOff)
    {
        if (_collider.enabled == onOff) return;

        _collider.enabled = onOff;
    }

    #region Animation control
    public void SetIdleAnim()
    {
        _myAnimator.SetBool("IsOnRange", false);
        _myAnimator.SetBool("IsCollected", false);

        _isNear = false;
        RefreshFX();
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

        _isNear = true;
        RefreshFX();
    }
    #endregion

    #region Shader control
    public void StartDisintegrate(Shader shader, Vector2 minMax, float alpha = 1f)
    {
        var mat = _renderer.material;
        mat.shader = shader;
        mat.SetFloat("_ColorController", 1);
        mat.SetColor("_StartingColor", Color.black);
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
    #endregion

    #region FX Helpers
    private void RefreshFX()
    {
        // Encender FX solo si está en rango y el nodo está Corrupted
        bool shouldPlay = _isNear && _currentType == NodeType.Corrupted;

        if (shouldPlay)
            PlayAllFX();
        else
            StopAllFX();
    }

    private void PlayAllFX()
    {
        if (_particles == null) return;
        foreach (var ps in _particles)
        {
            if (ps != null && !ps.isPlaying) ps.Play();
        }
    }

    private void StopAllFX()
    {
        if (_particles == null) return;
        foreach (var ps in _particles)
        {
            if (ps != null && ps.isPlaying) ps.Stop();
        }
    }
    #endregion
}