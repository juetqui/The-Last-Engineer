using UnityEngine;
using PrimeTween;

public class DoorLightListener : MonoBehaviour
{
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _defaultColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _openColor;

    [SerializeField] private float _lerpTime = 1f;
    [SerializeField] private Ease _lerpType;

    private DoorsView _door = default;
    private Renderer _renderer = default;

    private Color _targetColor;
    private Tween _tween;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _door = GetComponentInParent<DoorsView>();
        _door.OnOpen += ChangeLights;
    }

    private void ChangeLights(bool enabled)
    {
        var startColor = _renderer.material.GetColor("_EmissiveColor");
        var endColor = enabled ? _openColor : _defaultColor;

        _tween.Stop();

        _tween = Tween.Custom(gameObject, 0f, 1f, _lerpTime, (_, t) =>
            {
                var lerped = Color.Lerp(startColor, endColor, t);
                _renderer.material.SetColor("_EmissiveColor", lerped);
            }, _lerpType);
    }

    private void OnDestroy()
    {
        _tween.Stop();
    }
}
