using UnityEngine;

public class NodeRenderer : INodeEffect, INodeAudio
{
    private Renderer _renderer;
    private Collider _collider;
    private Color _color;
    private ParticleSystem _ps;
    private AudioSource _source;

    public NodeRenderer(Renderer renderer, Collider collider, Color color, ParticleSystem ps, AudioSource source)
    {
        _renderer = renderer;
        _collider = collider;
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
