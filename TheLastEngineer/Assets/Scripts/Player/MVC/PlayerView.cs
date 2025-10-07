using System;
using UnityEngine;

public class PlayerView
{
    private Renderer _renderer = default;
    private Material[] _originalMats = default, _corruptionMats = default;
    private ParticleSystem _walkPS = default, _orbitPS = default;
    private ParticleSystem _defaultPS = default, _corruptedPS = default;
    //private SolvingController _solvingController;
    private Animator _animator = default;
    private AudioSource _walkSource = default, _fxSource = default;
    private AudioClip _walkClip = default, _dashClip = default, _chargedDashClip = default, _liftClip = default, _putDownClip = default, _deathClip = default, _fallClip = default;

    private Color _defaultOutline = new Color(0, 0, 0, 0);

    public Action OnDashViewPlayed = delegate { };

    public PlayerView(Renderer renderer, ParticleSystem walkPS, ParticleSystem orbitPS, Animator animator, AudioSource walkSource, AudioSource fxSource, PlayerData playerData, ParticleSystem defaultPS, ParticleSystem corruptedPS)
    {
        if (renderer == null || playerData == null)//|| solvingController == null
            throw new System.ArgumentNullException("Core dependencies cannot be null");

        _renderer = renderer;
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
        _fallClip = playerData.fallClip;
        //_solvingController = solvingController;
        _defaultPS = defaultPS;
        _corruptedPS = corruptedPS;
    }

    public void OnStart()
    {
        _originalMats = _renderer.materials;
        _renderer.material.SetColor("_EmissiveColor", Color.black);

        var corruptionMat = Resources.Load<Material>("Materials/M_PlayerCorruption");
        
        _corruptionMats = new Material[_originalMats.Length];
        
        for (int i = 0; i < _corruptionMats.Length; i++)
            _corruptionMats[i] = corruptionMat;
    }


    public void Walk(Vector3 moveVector)
    {

        if (moveVector.magnitude > 0f)
        {
            _animator.SetBool("IsWalking", true);
            if (!_walkPS.isPlaying) _walkPS.Play();
            return;
        }

        _animator.SetBool("IsWalking", false);
        if (_walkPS.isPlaying) _walkPS.Stop();
    }

    public void DashSound()
    {
        OnDashViewPlayed?.Invoke();
        _walkPS.Stop();
        //SetParticlesLT(0.7f, 1f);
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
        //_solvingController.RespawnPlayer();
    }

    public void DashChargedSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _chargedDashClip, 3f);
    }

    public void DeathSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _deathClip, 1f);
    }

    public void FallSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _fallClip, 1f);
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
        //_orbitPS.Play();
    }

    public void PlayNodePS(NodeType nodeType)
    {
        if (nodeType == NodeType.Corrupted) _corruptedPS.Play();
        else _defaultPS.Play();
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
            PlayPS(outlineColor);
        else
            StopPS();
    }

    public void PlayErrorSound(AudioClip clip)
    {
        if (_fxSource.isPlaying) return;
        
        PlayAudioWithRandomPitch(_fxSource, clip);
    }

    private void PlayAudioWithRandomPitch(AudioSource source, AudioClip clip, float pitch = 0)
    {
        if (pitch == 0) pitch = UnityEngine.Random.Range(0.95f, 1.125f);

        source.clip = clip;
        source.pitch = pitch;
        source.Play();
    }
}
