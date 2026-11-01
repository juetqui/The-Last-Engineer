using PrimeTween;
using UnityEngine;

public class CauldronView : MonoBehaviour, IPivotMover
{
    [SerializeField] private Transform _pivot;
    [SerializeField] private float _stepDuration = 0.3f;

    private Tween _moveTween;

    public Vector3 Position => _pivot.position;

    public void MoveTo(Vector3 destination, Ease ease)
    {
        _moveTween.Stop();
        _moveTween = Tween.Position(_pivot, destination, _stepDuration, ease);
    }

    private void OnDestroy()
    {
        _moveTween.Stop();
    }
}
