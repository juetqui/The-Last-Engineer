using UnityEngine;
using UnityEngine.Splines;

public class MenuCamera : BaseCamera
{
    [SerializeField] private SplineAnimate _sAnimatePlay;
    [SerializeField] private SplineAnimate _sAnimateCredits;
    [SerializeField] private SplineAnimate _sAnimateExit;
    //[SerializeField] private SplineAnimate _sAnimateBack;

    private bool _animSet = false;

    private void Awake()
    {
        _basePos = transform.position;
    }

    private void Start()
    {
        _sAnimatePlay.Restart(false);
        Adjust();
    }

    private void LateUpdate()
    {
        if (!_animSet) ApplyBreathEffect();
    }

    public void PlayAnimationPlay()
    {
        _animSet = true;
        _sAnimatePlay.Play();
    }

    public void PlayAnimationCredits()
    {
        _animSet = true;
        _sAnimateCredits.Play();
    }

    public void PlayAnimationExit()
    {
        _animSet = true;
        _sAnimateExit.Play();
    }
    //public void PlayAnimationBack()
    //{
    //    _animSet = true;
    //    _sAnimateBack.Play();
    //}
}
