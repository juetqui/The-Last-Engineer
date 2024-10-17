using UnityEngine;

public class NodeRenderer : INodeEffect, INodeAudio
{
    private Renderer _renderer = default;
    private Collider _collider = default, _triggerCollider = default;
    private Color _color = default;
    private ParticleSystem _ps = default;
    private AudioSource _source = default;

    public NodeRenderer(Renderer renderer, Collider collider, Collider triggerCollider, Color color, ParticleSystem ps, AudioSource source)
    {
        _renderer = renderer;
        _collider = collider;
        _triggerCollider = triggerCollider;
        _color = color;
        _ps = ps;
        _source = source;
    }

    public void OnStart()
    {
        _renderer.material.color = _color;
        Enable(true);
        PlayEffect(false);
    }

    public void Enable(bool value)
    {
        _renderer.enabled = value;
        _collider.enabled = value;
    }

    public void EnableTrigger(bool value)
    {
        _triggerCollider.enabled = value;
    }

    public void ChangeColor(Color color)
    {
        _renderer.material.color = color;
    }

    public void PlayEffect(bool turnOnOff)
    {
        _ps.gameObject.SetActive(turnOnOff);
    }

    public void PlayClip(AudioClip audio)
    {
        _source.clip = audio;
        _source.Play();
    }
}
