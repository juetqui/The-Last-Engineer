using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldView
{
    private Renderer _renderer = default;
    private SphereCollider _collider = default;
    private AudioSource _audioSource = default;

    public ShieldView(Renderer renderer, SphereCollider collider, AudioSource audioSource, AudioClip chargedFX)
    {
        _renderer = renderer;
        _collider = collider;
        _audioSource = audioSource;
        _audioSource.clip = chargedFX;
    }

    public void SetActive(bool activate)
    {
        _renderer.enabled = activate;
        _collider.enabled = activate;
    }

    public void PlayChargedFX()
    {
        _audioSource.pitch = Random.Range(1.6f, 2f);

        if (!_audioSource.isPlaying)
            _audioSource?.Play();
    }
}
