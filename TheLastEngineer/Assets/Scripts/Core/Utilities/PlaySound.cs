using System;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            _audioSource.Play();
    }
}
