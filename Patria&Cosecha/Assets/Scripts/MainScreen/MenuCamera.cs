using UnityEngine;
using UnityEngine.Splines;

public class MenuCamera : BaseCamera
{
    [SerializeField] private SplineAnimate _sAnimate;

    private bool _animSet = false;

    private void Awake()
    {
        _basePos = transform.position;
    }

    private void Start()
    {
        _sAnimate.enabled = false;
        Adjust();
    }

    private void LateUpdate()
    {
        if (!_animSet) ApplyBreathEffect();
    }

    public void PlayAnimation()
    {
        _animSet = true;
        _sAnimate.enabled = true;
        _sAnimate.Play();
    }
}
