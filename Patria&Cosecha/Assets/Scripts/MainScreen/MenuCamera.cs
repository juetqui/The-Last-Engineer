using UnityEngine;
using UnityEngine.Splines;

public class MenuCamera : BaseCamera
{
    [SerializeField] private SplineAnimate _sAnimatePlay;
    [SerializeField] private SplineAnimate _sAnimateCredits;
    [SerializeField] private SplineAnimate _sAnimateExit;

    private bool _animSet = false;

    private void Awake()
    {
        _basePos = transform.position;
    }

    private void Start()
    {
        _sAnimatePlay.enabled = false;
        _sAnimateCredits.enabled = false;
        _sAnimateExit.enabled = false;
        Adjust();
    }

    private void LateUpdate()
    {
        if (!_animSet) ApplyBreathEffect();
    }

    public void PlayAnimationPlay()
    {
        _animSet = true;
        _sAnimatePlay.enabled = true;
        _sAnimatePlay.Play();
    }

    public void PlayAnimationCredits()
    {
        _animSet = true;
        _sAnimateCredits.enabled = true;
        _sAnimateCredits.Play();
    }

    public void PlayAnimationExit()
    {
        _animSet = true;
        _sAnimateExit.enabled = true;
        _sAnimateExit.Play();
    }
}
