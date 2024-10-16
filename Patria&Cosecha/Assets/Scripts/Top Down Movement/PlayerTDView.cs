using UnityEngine;

public class PlayerTDView
{
    private AudioSource _source = default;

    public PlayerTDView(AudioSource source)
    {
        _source = source;
    }

    public void OnUpdate()
    {
        
    }

    public float WalkSound(ref float time)
    {
        if (!_source.isPlaying)
        {
            float pitch = Random.Range(0.85f, 1.25f);
            
            _source.pitch = pitch;
            _source.Play();
        }
        
        return 0;
    }
}
