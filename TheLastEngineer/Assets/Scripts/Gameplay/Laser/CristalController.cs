using UnityEngine;

public class CristalController : MonoBehaviour
{
    private static readonly int GlitchAmountId = Shader.PropertyToID("_GlitchAmount");
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");

    private const float On = 1f;
    private const float Off = 0f;

    // Glitcheable and Intangible lasers both show the glitch effect: the player can swap their state.
    private bool _isSwapTarget;
    private Material _material;

    // Resolved exactly like LaserController.EmitterState, so the crystal and its beams never disagree.
    private GlitchComponent _glitch;

    private GlitchState EmitterState => _glitch != null ? _glitch.CurrentState : GlitchState.Clean;

    private void Awake()
    {
        _isSwapTarget = GetComponentInParent<IGlitchSwapTarget>() != null;
        _glitch = GetComponentInParent<GlitchComponent>();

        // renderer.material makes a per-instance copy: cache it once instead of on every setter.
        _material = GetComponent<Renderer>().material;
    }

    private void Start()
    {
        SetGlitcheableCristalView();

        // GlitchComponent never raises its initial state: pull it here, then follow the changes.
        ApplyEmissiveColor(EmitterState);
        if (_glitch != null) _glitch.OnGlitchStateChanged += ApplyEmissiveColor;
    }

    private void OnDestroy()
    {
        if (_glitch != null) _glitch.OnGlitchStateChanged -= ApplyEmissiveColor;
    }

    public void SetGlitcheableCristalView()
    {
        _material.SetFloat(GlitchAmountId, _isSwapTarget ? On : Off);
    }

    private void ApplyEmissiveColor(GlitchState state)
    {
        _material.SetColor(EmissiveColorId, GlitchPalette.Default.EmissionFor(state));
    }
}
