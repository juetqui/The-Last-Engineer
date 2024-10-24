using UnityEngine;

public class PlayerTDView
{
    private Outline _outline = default;
    private ParticleSystem _ps = default;

    private Animator _animator = default;
    private AudioSource _source = default;
    private AudioClip _walkClip = default, _grabClip = default;

    private float _timer = default, _interval = 0.0125f;
    private Color _defaultOutline = new Color(0, 0, 0, 0);

    public PlayerTDView(Outline outline, ParticleSystem ps, Animator animator, AudioSource source, AudioClip walkClip, AudioClip grabClip)
    {
        _outline = outline;
        _ps = ps;
        _animator = animator;
        _source = source;
        _walkClip = walkClip;
        _grabClip = grabClip;
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

    public void PlayDashPS()
    {
        _ps.Play();
    }

    public void StopDashPS()
    {
        _ps.Stop();
    }

    public void GrabNode(Color outlineColor = default)
    {
        if (!_source.isPlaying)
        {
            _source.clip = _grabClip;
            _source.Play();
        }

        if (outlineColor != Color.black)
        {
            _outline.OutlineColor = outlineColor;
            _outline.OutlineWidth = 5;
        }
        else
        {
            _outline.OutlineColor = _defaultOutline;
            _outline.OutlineWidth = 0;
        }
    }
}
