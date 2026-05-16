using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class CinematicSpaceshipController : MonoBehaviour
{
    private Animator _animator;
    private AudioSource _source;
    private SplineAnimate _splineAnimate;

    public Action<CameraShakeType> OnStartAnimation;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _source = GetComponent<AudioSource>();
        _splineAnimate = GetComponent<SplineAnimate>();
    }

    private void Start()
    {
        InputManager.Instance.dashInput.performed += StartAnimation;
    }

    public void StartAnimation(InputAction.CallbackContext context)
    {
        _animator.SetTrigger("StartAnimation");
        _source.Play();
        OnStartAnimation?.Invoke(CameraShakeType.Strong);
    }

    public void AnimationEnded()
    {
        OnStartAnimation?.Invoke(CameraShakeType.Stop);
    }

    public void StartSplineAnimation()
    {
        _splineAnimate.Play();
        OnStartAnimation?.Invoke(CameraShakeType.Weak);
    }
}
