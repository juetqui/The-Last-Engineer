using UnityEngine;

public class DoorLightListener : MonoBehaviour
{
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _defaultColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _openColor;

    [SerializeField] private float _lerpTime = 1f;
    [SerializeField] private LeanTweenType _lerpType;

    private DoorsView _door = default;
    private Renderer _renderer = default;

    private Color _targetColor;

    private bool _isLerping = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _door = GetComponentInParent<DoorsView>();
        _door.OnOpen += ChangeLights;
    }

    private void ChangeLights(bool enabled)
    {
        Color startColor = _renderer.material.GetColor("_EmissiveColor");
        Color endColor = enabled ? _openColor : _defaultColor;

        LeanTween.cancel(gameObject);

        LeanTween.value(gameObject, 0f, 1f, _lerpTime)
            .setEase(_lerpType)
            .setOnUpdate((float t) =>
            {
                Color lerped = Color.Lerp(startColor, endColor, t);
                _renderer.material.SetColor("_EmissiveColor", lerped);
            });
    }
}
