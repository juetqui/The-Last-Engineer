using UnityEngine;

public sealed class NodeViewMono : MonoBehaviour, INodeView
{
    [Header("Refs")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;
    [SerializeField] private Outline _outline;
    [SerializeField] private Animator _animator;
    [SerializeField] private ParticleSystem[] _proximityFX;

    public void Initialize(Color outlineColor)
    {
        if (_outline) _outline.OutlineColor = outlineColor;
        SetIdle();
    }

    public void EnableCollider(bool on)
    {
        if (_collider && _collider.enabled != on) _collider.enabled = on;
    }

    public void SetIdle()
    {
        if (_animator)
        {
            _animator.SetBool("IsOnRange", false);
            _animator.SetBool("IsCollected", false);
        }
        PlayProximityFX(false);
    }

    public void SetCollected()
    {
        if (_animator)
        {
            _animator.SetBool("IsOnRange", false);
            _animator.SetBool("IsCollected", true);
        }
        PlayProximityFX(false);
    }

    public void SetInRange()
    {
        if (_animator)
        {
            _animator.SetBool("IsOnRange", true);
            _animator.SetBool("IsCollected", false);
        }
        // No activamos FX acá; lo controla el Controller según tipo/cercanía
    }

    public void SetOutlineColor(Color c)
    {
        if (_outline) _outline.OutlineColor = c;
        if (_renderer && _renderer.material.HasProperty("_EmissiveColor"))
            _renderer.material.SetColor("_EmissiveColor", c);
    }

    public void PlayProximityFX(bool play)
    {
        if (_proximityFX == null) return;
        foreach (var ps in _proximityFX)
        {
            if (!ps) continue;
            if (play && !ps.isPlaying) ps.Play();
            else if (!play && ps.isPlaying) ps.Stop();
        }
    }

    public void StartDisintegrate(Shader shader, Color startColor, Vector2 minMax, float alpha = 1f)
    {
        var mat = _renderer.material;
        mat.shader = shader;
        mat.SetFloat("_ColorController", 1);
        mat.SetColor("_StartingColor", startColor);
        mat.SetVector("_MinMaxPos", minMax);
        mat.SetFloat("_Alpha", alpha);
        if (_outline) _outline.enabled = false;
    }

    public void SetDisintegrateAlpha(float alpha)
    {
        _renderer.material.SetFloat("_Alpha", alpha);
    }

    public void StopDisintegrate(Shader originalShader)
    {
        var mat = _renderer.material;
        mat.shader = originalShader;
        if (_outline) _outline.enabled = true;
    }
}
