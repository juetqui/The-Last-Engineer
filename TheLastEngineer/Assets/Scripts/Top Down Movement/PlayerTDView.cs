using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTDView
{
    private Renderer _renderer = default;
    private Material[] _originalMats = default, _corruptionMats = default;
    private Outline _outline = default;
    private ParticleSystem _walkPS = default, _orbitPS = default;
    private SolvingController _solvingController;
    private Animator _animator = default;
    private AudioSource _walkSource = default, _fxSource = default;
    private AudioClip _walkClip = default, _dashClip = default, _chargedDashClip = default, _liftClip = default, _putDownClip = default, _deathClip;
    private Image _dashImage = default;

    private Color _defaultOutline = new Color(0, 0, 0, 0);

    public PlayerTDView(Renderer renderer, Outline outline, ParticleSystem walkPS, ParticleSystem orbitPS, Animator animator, AudioSource walkSource, AudioSource fxSource, PlayerData playerData, SolvingController solvingController, Image dashImage)
    {
        _renderer = renderer;
        _outline = outline;
        _walkPS = walkPS;
        _orbitPS = orbitPS;
        _animator = animator;
        _walkSource = walkSource;
        _fxSource = fxSource;
        _walkClip = playerData.walkClip;
        _dashClip = playerData.dashClip;
        _chargedDashClip = playerData.chargedDashClip;
        _liftClip = playerData.liftClip;
        _putDownClip = playerData.putDownClip;
        _deathClip = playerData.deathClip;
        _solvingController = solvingController;
        _dashImage = dashImage;
    }

    public void OnStart()
    {
        _originalMats = _renderer.materials;
        _corruptionMats = new Material[_renderer.materials.Length];

        for (int i = 0; i < _renderer.materials.Length; i++)
        {
            _corruptionMats[i] = Resources.Load<Material>("Materials/M_PlayerCorruption");
        }
    }

    public void Walk(Vector3 moveVector)
    {
        if (moveVector.magnitude > 0f) _animator.SetBool("IsWalking", true);
        else _animator.SetBool("IsWalking", false);
    }

    public void DashSound()
    {
        if(_dashImage!=null)
        _dashImage.fillAmount = 0f;
        SetParticlesLT(0.7f, 1f);
        PlayAudioWithRandomPitch(_fxSource, _dashClip);
    }
    
    public void SetAnimatorSpeed(float speed)
    {
        _animator.speed = speed;
    }

    public void UpdatePlayerMaterials(bool hasPowerUp)
    {
        if (hasPowerUp) _renderer.materials = _corruptionMats;
        else _renderer.materials = _originalMats;
    }

    public void RespawnPlayer()
    {
        _animator.speed = 1;
        _solvingController.RespawnPlayer();
    }

    public void DashChargedSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _chargedDashClip, 3f);
    }

    public void DeathSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _deathClip, 1f);
    }

    public void WalkSound()
    {
        PlayAudioWithRandomPitch(_walkSource, _walkClip);
    }

    public void PlayPS(Color color)
    {
        var walkPS = _walkPS.main;
        var orbitPS = _orbitPS.main;

        walkPS.startColor = color;
        orbitPS.startColor = color;
        _orbitPS.Play();
    }

    public void StopPS()
    {
        var walkPS = _walkPS.main;
        walkPS.startColor = Color.white;
        _orbitPS.Stop();
    }

    private void SetParticlesLT(float minVal, float maxVal)
    {
        var main = _walkPS.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(minVal, maxVal);
    }

    public void GrabNode(bool grab, Color outlineColor)
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

    public IEnumerator DashCD(float cd)
    {
        float timer = 0f;

        while (timer < cd)
        {
            timer += Time.deltaTime;
            if (_dashImage != null)

                _dashImage.fillAmount = timer / cd;
            yield return null;
        }
        if (_dashImage != null)

            _dashImage.fillAmount = 1f;
        SetParticlesLT(0.2f, 0.3f);
    }
}
