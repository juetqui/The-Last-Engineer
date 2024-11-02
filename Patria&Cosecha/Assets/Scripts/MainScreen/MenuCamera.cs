using UnityEngine;
using UnityEngine.Splines;

public class MenuCamera : CameraTDController
{
    [SerializeField] private SplineAnimate _sAnimate;

    public void PlayAnimation()
    {
        _sAnimate.Play();
    }
}
