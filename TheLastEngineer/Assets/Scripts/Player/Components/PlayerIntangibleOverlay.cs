using PrimeTween;
using UnityEngine;

/// <summary>
/// Fades the "_Alpha" of the Intangible overlay material on one player renderer (body or backpack)
/// while the player carries an Intangible node. One instance per renderer, all listening to the same
/// PlayerNodeHandler.OnNodeGrabbed with the same timing, so body and backpack stay in sync by construction.
/// </summary>
public class PlayerIntangibleOverlay : MonoBehaviour
{
    // Time for a full 0 -> 1 (or 1 -> 0) fade. Partial fades take proportionally less.
    private const float FullFadeDuration = 0.5f;
    private const float VisibleAlpha = 1f;
    private const float HiddenAlpha = 0f;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    [SerializeField] private Renderer _renderer;
    [Tooltip("Renderer material slot that holds the Intangible overlay (shader with an _Alpha property).")]
    [SerializeField] private int _materialIndex = 1;
    [Tooltip("Optional. If empty, it is looked up in the parents, then PlayerNodeHandler.Instance is used.")]
    [SerializeField] private PlayerNodeHandler _nodeHandler;

    private Material _overlayMat;
    private Tween _fadeTween;
    private bool _subscribed;

    private void Reset()
    {
        _renderer = GetComponent<Renderer>();
        _nodeHandler = GetComponentInParent<PlayerNodeHandler>();
    }

    private void Awake()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_nodeHandler == null) _nodeHandler = GetComponentInParent<PlayerNodeHandler>();

        // Cached once: reading Renderer.materials again while the corruption materials are on would
        // instantiate copies of those instead of returning this overlay instance.
        _overlayMat = ResolveOverlayMaterial();
    }

    // In Start and not in Awake: PlayerNodeHandler.Instance is set in its own Awake.
    private void Start()
    {
        if (_nodeHandler == null) _nodeHandler = PlayerNodeHandler.Instance;

        if (_overlayMat == null || _nodeHandler == null)
        {
            Debug.LogWarning($"{nameof(PlayerIntangibleOverlay)} on '{name}' has no overlay material or PlayerNodeHandler; disabling.", this);
            enabled = false;
            return;
        }

        _nodeHandler.OnNodeGrabbed += OnNodeGrabbed;
        _subscribed = true;

        // Initial look without animating, read by pull: nothing raises the starting state.
        SetAlpha(TargetAlpha(_nodeHandler.HasNode, _nodeHandler.CurrentLevel));
    }

    private void OnDestroy()
    {
        if (_subscribed && _nodeHandler != null) _nodeHandler.OnNodeGrabbed -= OnNodeGrabbed;
        if (_fadeTween.isAlive) _fadeTween.Stop();
    }

    // Raised on pick, release and every level change of the node in hand.
    private void OnNodeGrabbed(bool hasNode, GlitchState level)
    {
        if (_overlayMat == null) return;

        if (_fadeTween.isAlive) _fadeTween.Stop();

        float current = _overlayMat.GetFloat(AlphaId);
        float target = TargetAlpha(hasNode, level);
        float duration = FullFadeDuration * Mathf.Abs(target - current);

        if (duration <= 0f)
        {
            SetAlpha(target);
            return;
        }

        _fadeTween = Tween.Custom(this, current, target, duration, (self, alpha) => self.SetAlpha(alpha), Ease.Linear);
    }

    private static float TargetAlpha(bool hasNode, GlitchState level) =>
        hasNode && level == GlitchState.Intangible ? VisibleAlpha : HiddenAlpha;

    private void SetAlpha(float alpha)
    {
        if (_overlayMat != null) _overlayMat.SetFloat(AlphaId, alpha);
    }

    private Material ResolveOverlayMaterial()
    {
        if (_renderer == null) return null;

        var mats = _renderer.materials;
        if (_materialIndex < 0 || _materialIndex >= mats.Length) return null;

        var mat = mats[_materialIndex];
        return mat != null && mat.HasProperty(AlphaId) ? mat : null;
    }
}
