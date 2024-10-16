using UnityEngine;

public class PlayerTDView
{
    private AudioSource _source = default;
    private AudioClip _walkClip = default, _grabClip = default;

    public PlayerTDView(AudioSource source, AudioClip walkClip, AudioClip grabClip)
    {
        _source = source;
        _walkClip = walkClip;
        _grabClip = grabClip;
    }

    public void OnUpdate()
    {
        
    }

    public float WalkSound(ref float time)
    {
        if (_source.isPlaying) return 0;
            
        float pitch = Random.Range(0.85f, 1.25f);

        _source.clip = _walkClip;
        _source.pitch = pitch;
        _source.Play();
        
        return 0;
    }

    public void GrabNode()
    {
        if (_source.isPlaying) return;

        _source.clip = _grabClip;
        _source.Play();
    }
}
