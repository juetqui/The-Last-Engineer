using PrimeTween;
using UnityEngine;

/// <summary>
/// Ghost silhouette of an Intangible: fades its _Alpha to _targetAlpha while the Intangible looks phased and
/// back to 0 when it looks solid. Follows Intangible.OnPhaseVisualChanged, so it also waits for the player to
/// leave before fading out (Exiting state).
/// </summary>
public class IntangibleGhost : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Intangible _intangible;
    [SerializeField] private Renderer _renderer;

    [Header("Visual")]
    [Tooltip("_Alpha while the Intangible is phased.")]
    [SerializeField, Range(0f, 1f)] private float _targetAlpha = 1f;
    [SerializeField] private float _fadeTime = 0.4f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    private const float HiddenAlpha = 0f;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    private Material _material;
    private Tween _fadeTween;

    private void Awake()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogWarning($"[IntangibleGhost] {name}: no Renderer", this);
            return;
        }

        // Own copy: writing the shared .mat would change every ghost.
        _material = _renderer.material;
        if (!_material.HasProperty(AlphaId))
            Debug.LogWarning($"[IntangibleGhost] {name}: material {_material.name} has no _Alpha", this);
    }

    private void OnEnable()
    {
        if (_intangible == null)
        {
            Debug.LogWarning($"[IntangibleGhost] {name}: no Intangible assigned", this);
            return;
        }

        _intangible.OnPhaseVisualChanged += OnPhaseVisualChanged;
    }

    private void OnDisable()
    {
        if (_intangible != null) _intangible.OnPhaseVisualChanged -= OnPhaseVisualChanged;
    }

    // Initial look without animating, read by pull: the Intangible may run its Start after this one.
    private void Start()
    {
        if (_intangible != null) SetAlpha(TargetFor(_intangible.IsVisuallyPhased));
    }

    private void OnDestroy()
    {
        if (_fadeTween.isAlive) _fadeTween.Stop();
        if (_material != null) Destroy(_material);
    }

    private void OnPhaseVisualChanged(bool phased)
    {
        if (_fadeTween.isAlive) _fadeTween.Stop();

        float target = TargetFor(phased);
        if (_material == null || _fadeTime <= 0f)
        {
            SetAlpha(target);
            return;
        }

        _fadeTween = Tween.Custom(this, _material.GetFloat(AlphaId), target, _fadeTime, (ghost, a) => ghost.SetAlpha(a), _ease);
    }

    private float TargetFor(bool phased) => phased ? _targetAlpha : HiddenAlpha;

    private void SetAlpha(float alpha)
    {
        if (_material != null) _material.SetFloat(AlphaId, alpha);
    }
}
