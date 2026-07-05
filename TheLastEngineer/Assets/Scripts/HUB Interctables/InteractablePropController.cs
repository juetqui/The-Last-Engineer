using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class InteractablePropController : MonoBehaviour, IInteractable
{
    #region INTERFACE VARIABLES
    public InteractablePriority Priority => InteractablePriority.Low;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion
    
    [Header("Audios")]
    [SerializeField] private AudioClip commonAudio;
    [SerializeField] private AudioClip chanceAudio;
    [SerializeField] private AudioClip brokenAudio;

    [Header("Chance Values")]
    [SerializeField] private Vector2 pitchMinMax;
    [SerializeField] private Vector2Int audioMinMax;
    [SerializeField] private float randomChance;
    [SerializeField] private float breakChance;

    // private Animator _animator;
    private AudioSource _audioSource;
    private AudioClip _targetAudio;

    private void OnValidate()
    {
        if (randomChance < audioMinMax.x)
            randomChance =  audioMinMax.x;
        else if (randomChance > audioMinMax.y)
            randomChance =  audioMinMax.y;
        
        if (breakChance < audioMinMax.x)
            breakChance =  audioMinMax.x;
        else if (breakChance > audioMinMax.y)
            breakChance =  audioMinMax.y;
    }

    private void Awake()
    {
        // _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = commonAudio;
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler)
    {
        return !_audioSource.isPlaying;
    }

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool interactionSucceeded)
    {
        var pitch = Random.Range(pitchMinMax.x, pitchMinMax.y);
        var chance = Random.Range(audioMinMax.x, audioMinMax.y);

        var target = chance < randomChance ? chanceAudio : commonAudio;
        _targetAudio = chance > breakChance ? brokenAudio: target;

        // _animator.SetTrigger("Interact");
        _audioSource.pitch = pitch;
        _audioSource.clip = _targetAudio;
        _audioSource.Play();
        interactionSucceeded = true;
    }
}
