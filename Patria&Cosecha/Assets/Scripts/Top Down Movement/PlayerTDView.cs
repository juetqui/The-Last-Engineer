using UnityEngine;

public class PlayerTDView
{
    private Outline _outline = default;
    private ParticleSystem[] _ps = default;

    private Animator _animator = default;
    private AudioSource _source = default;
    private AudioClip _walkClip = default, _liftClip = default, _putDownClip = default;

    private float _timer = default, _interval = 0.0125f;
    private Color _defaultOutline = new Color(0, 0, 0, 0);

    public PlayerTDView(Outline outline, ParticleSystem[] ps, Animator animator, AudioSource source, AudioClip walkClip, AudioClip liftClip, AudioClip putDownClip)
    {
        _outline = outline;
        _ps = ps;
        _animator = animator;
        _source = source;
        _walkClip = walkClip;
        _liftClip = liftClip;
        _putDownClip = putDownClip;
    }

    public void Walk(Vector3 moveVector)
    {
        if (moveVector.x != 0 || moveVector.z != 0)
        {
            _animator.SetBool("IsWalking", true);
            WalkSound();
        }
        else _animator.SetBool("IsWalking", false);
    }

    private void WalkSound()
    {
        if (!_source.isPlaying)
        {
            _source.volume = 0.6f;
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

    public void PlayPS(Color color)
    {
        foreach (var ps in _ps)
        {
            ps.startColor = color;
            ps.Play();
        }
    }

    public void StopPS()
    {
        foreach (var ps in _ps) ps.Stop();
    }

    public void GrabNode(bool grab = false, Color outlineColor = default)
    {
        if (grab) _source.clip = _liftClip;
        else _source.clip = _putDownClip;
        
        _source.volume = 1f;
        _source.Play();

        if (outlineColor != Color.black)
        {
            PlayPS(outlineColor);
            _outline.OutlineColor = outlineColor;
            _outline.OutlineWidth = 5;
        }
        else
        {
            StopPS();
            _outline.OutlineColor = _defaultOutline;
            _outline.OutlineWidth = 0;
        }
    }
}
