using UnityEngine;

public class PlayerTDView
{
    private AudioSource _source = default;
    private AudioClip _walkClip = default, _grabClip = default;

    private float _timer = default, _interval = 0.025f;

    public PlayerTDView(AudioSource source, AudioClip walkClip, AudioClip grabClip)
    {
        _source = source;
        _walkClip = walkClip;
        _grabClip = grabClip;
    }

    public void WalkSound(Vector3 moveVector)
    {
        if (moveVector.x != 0 && !_source.isPlaying || moveVector.z != 0 && !_source.isPlaying)
        {
            _timer += Time.deltaTime;

            if (_timer >= _interval)
            {
                float pitch = Random.Range(1f, 1.5f);

                _source.clip = _walkClip;
                _source.pitch = pitch;
                _source.Play();
            }
        }
        else _timer = 0;
    }

    public void GrabNode()
    {
        if (_source.isPlaying) return;

        _source.clip = _grabClip;
        _source.Play();
    }
}
