using System;
using UnityEngine;
using PrimeTween;

public class PlayerView
{
    private GameObject _playerObj;
    private Renderer _renderer = default;
    private Material[] _originalMats = default, _corruptionMats = default;
    private ParticleSystem _walkPS = default, _orbitPS = default;
    private ParticleSystem _defaultPS = default, _corruptedPS = default;
    private ParticleSystem.MinMaxGradient _defaultPSColor = default;
    private ParticleSystem _teleportPS = default;
    private Animator _animator = default;
    private AudioSource _walkSource = default, _fxSource = default;
    private AudioClip _walkClip = default, _dashClip = default, _chargedDashClip = default, _liftClip = default, _putDownClip = default, _deathClip = default, _fallClip = default;
    private Material _intangibleMat;

    private Color _defaultOutline = new Color(0, 0, 0, 0);

    public Action OnDashViewPlayed = delegate { };

    public PlayerView(GameObject playerObj, Renderer renderer, ParticleSystem walkPS, ParticleSystem orbitPS, Animator animator, AudioSource walkSource, AudioSource fxSource, PlayerData playerData, ParticleSystem defaultPS, ParticleSystem corruptedPS, ParticleSystem teleportPS)
    {
        if (renderer == null || playerData == null)
            throw new System.ArgumentNullException("Core dependencies cannot be null");

        _playerObj = playerObj;
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
        _defaultPS = defaultPS;
        _corruptedPS = corruptedPS;
        _teleportPS = teleportPS;
        _intangibleMat = playerData.intangibleMat;

        if (_defaultPS != null) _defaultPSColor = _defaultPS.main.startColor;
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
     
        SFXManager.Instance.PlaySFX(_dashClip, _playerObj.transform, 1f, false);   
        // PlayAudioWithRandomPitch(_fxSource, _dashClip);
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
    }

    public void DashChargedSound()
    {
        PlayAudioWithRandomPitch(_fxSource, _chargedDashClip, 3f);
    }

    public void DeathSound()
    {
        SFXManager.Instance.PlaySFX(_deathClip, _playerObj.transform, 1f, false);
        // PlayAudioWithRandomPitch(_fxSource, _deathClip, 1f);
    }

    public void FallSound()
    {
        SFXManager.Instance.PlaySFX(_fallClip, _playerObj.transform, 1f, false);
        // PlayAudioWithRandomPitch(_fxSource, _fallClip, 1f);
    }

    public void WalkSound()
    {
        SFXManager.Instance.PlaySFX(_walkClip, _playerObj.transform, 0.25f, false);
        // PlayAudioWithRandomPitch(_walkSource, _walkClip);
    }

    public void PlayPS(Color color)
    {
        var walkPS = _walkPS.main;
        var orbitPS = _orbitPS.main;

        walkPS.startColor = color;
        orbitPS.startColor = color;
        _orbitPS.Play();
    }

    public void PlayNodePS(GlitchState nodeType)
    {
        _renderer.materials[1].SetFloat("_HasNode", 1);

        var intangibleMatFrom = _intangibleMat.GetFloat("_Alpha");
        
        if (nodeType == GlitchState.Intangible)
        {
            TweenFloat(_intangibleMat, "_Alpha", intangibleMatFrom, 1f, 0.5f);
        }
        else if (nodeType == GlitchState.Glitched)
        {
            TweenFloat(_intangibleMat, "_Alpha", intangibleMatFrom, 0f, 0.5f);
            _renderer.materials[1].SetFloat("_IsGlitched", 1);
            _corruptedPS.Play();
            return;
        }
        else
        {
            TweenFloat(_intangibleMat, "_Alpha", intangibleMatFrom, 0f, 0.5f);
        }

        // _IsGlitched es un booleano en S_GlowCharacter, así que Intangible se ve como Clean en el
        // material. Lo que lo distingue son las partículas: las de Clean teñidas con el color de la
        // paleta, restaurando el color original del prefab cuando se vuelve a Clean.
        _renderer.materials[1].SetFloat("_IsGlitched", 0);

        var main = _defaultPS.main;
        main.startColor = nodeType == GlitchState.Intangible
            ? new ParticleSystem.MinMaxGradient(GlitchPalette.Default.ColorFor(GlitchState.Intangible))
            : _defaultPSColor;

        _defaultPS.Play();
    }

    public void TeleportPS()
    {
        _teleportPS.Play();
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
            SFXManager.Instance.PlaySFX(_liftClip, _playerObj.transform, 1f, false);
            // PlayAudioWithRandomPitch(_fxSource, _liftClip);
        else
        {
            SFXManager.Instance.PlaySFX(_putDownClip, _playerObj.transform, 1f, false);
            // PlayAudioWithRandomPitch(_fxSource, _putDownClip);

            // SACAR ESTO DE ACA
            _renderer.materials[1].SetFloat("_HasNode", 0);
            _renderer.materials[1].SetFloat("_IsGlitched", 0);
        }

        if (outlineColor != Color.black)
            PlayPS(outlineColor);
        else
            StopPS();
    }

    public void PlayErrorSound(AudioClip clip)
    {
        if (_fxSource.isPlaying) return;
        
        SFXManager.Instance.PlaySFX(clip, _playerObj.transform, 1f);
        // PlayAudioWithRandomPitch(_fxSource, clip);
    }

    private void PlayAudioWithRandomPitch(AudioSource source, AudioClip clip, float pitch = 0)
    {
        if (pitch == 0) pitch = UnityEngine.Random.Range(0.95f, 1.125f);

        source.clip = clip;
        source.pitch = pitch;
        source.Play();
    }
    
    private Tween TweenFloat(Material mat, string prop, float from, float to, float dur)
    {
        mat.SetFloat(prop, from);
        return Tween.Custom(_playerObj, from, to, dur,
            (_, v) => mat.SetFloat(prop, v), Ease.Linear);
    }
}
