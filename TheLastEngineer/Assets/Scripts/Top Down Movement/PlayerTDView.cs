using UnityEngine;

public class PlayerTDView
{
    private Outline _outline = default;
    private ParticleSystem[] _ps = default;

    private Animator _animator = default;
    private AudioSource _walkSource = default, _fxSource = default;
    private AudioClip _walkClip = default, _dashClip = default, _chargedDashClip = default, _liftClip = default, _putDownClip = default;

    private Color _defaultOutline = new Color(0, 0, 0, 0);

    public PlayerTDView(Outline outline, ParticleSystem[] ps, Animator animator, AudioSource walkSource, AudioSource fxSource, PlayerData playerData)
    {
        _outline = outline;
        _ps = ps;
        _animator = animator;
        _walkSource = walkSource;
        _fxSource = fxSource;
        _walkClip = playerData.walkClip;
        _dashClip = playerData.dashClip;
        _chargedDashClip = playerData.chargedDashClip;
        _liftClip = playerData.liftClip;
        _putDownClip = playerData.putDownClip;
    }

    public void Walk(Vector3 moveVector)
    {
        Debug.Log(moveVector.magnitude);

        if (moveVector.magnitude > 0f) _animator.SetBool("IsWalking", true);
        else _animator.SetBool("IsWalking", false);
    }

    public void DashSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _dashClip);
    }
    public void DashChargedSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _chargedDashClip, 3f);
    }

    public void WalkSound()
    {
        PlayAudioWithRandomPitch(_walkSource, _walkClip);
    }

    public void PlayPS(Color color)
    {
        foreach (var ps in _ps)
        {
            ParticleSystem.MainModule psMain = ps.main;

            psMain.startColor = color;
            ps.Play();
        }
    }

    public void StopPS()
    {
        foreach (var ps in _ps) ps.Stop();
    }

    public void GrabNode(bool grab = false, Color outlineColor = default)
    {
        _fxSource.volume = 1f;

        if (grab)
            PlayAudioWithRandomPitch(_fxSource, _liftClip);
        else
            PlayAudioWithRandomPitch(_fxSource, _putDownClip);

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

    public void PlayErrorSound(AudioClip clip)
    {
        if (_fxSource.isPlaying) return;
        
        PlayAudioWithRandomPitch(_fxSource, clip);
    }

    private void PlayAudioWithRandomPitch(AudioSource source, AudioClip clip, float pitch = 0)
    {
        if (pitch == 0) pitch = Random.Range(0.95f, 1.125f);

        source.clip = clip;
        source.pitch = pitch;
        source.Play();
    }
}
