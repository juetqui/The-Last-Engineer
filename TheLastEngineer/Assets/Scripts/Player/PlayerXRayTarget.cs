using PrimeTween;
using UnityEngine;

/// <summary>
/// Marca los renderers del jugador con la Rendering Layer que dibuja PlayerXRayFeature (outline +
/// relleno cuando una pared lo tapa) y controla la intensidad global del efecto.
///
/// El efecto se apaga con fade al morir y durante cinematicas, y se desvanece junto con el dissolve
/// del teleport: el override material de la mascara no tiene el alpha clip del dissolve, asi que
/// sin esto se veria la silueta entera mientras el jugador se desintegra.
///
/// El color sale de GlitchPalette segun el nivel del nodo en mano (Clean sin nodo). Se pasa por
/// globales y no con SetColor sobre M_PlayerXRay: el material es un asset y el cambio quedaria
/// guardado en el .mat dentro del Editor.
/// </summary>
public class PlayerXRayTarget : MonoBehaviour
{
    private static readonly int XRayIntensityId = Shader.PropertyToID("_XRayIntensity");
    private static readonly int XRayOutlineTintId = Shader.PropertyToID("_XRayOutlineTint");
    private static readonly int XRayFillTintId = Shader.PropertyToID("_XRayFillTint");

    /// <summary>Targets con intensidad > 0. Con 0 la feature no encola ningun pass.</summary>
    public static int ActiveCount { get; private set; }

    [SerializeField] private PlayerController _player;
    [SerializeField] private PlayerNodeHandler _nodeHandler;
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private RenderingLayerMask _xrayLayer;

    [Header("Fade")]
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private Ease _fadeEase = Ease.OutQuad;

    [Header("Color (GlitchPalette segun el nodo en mano)")]
    [SerializeField] private float _colorDuration = 0.2f;
    [SerializeField] private Ease _colorEase = Ease.OutQuad;
    [Tooltip("El outline usa el color emisivo (HDR) de la paleta, para que el bloom lo levante como al outline del nodo.")]
    [SerializeField] private bool _useEmissionForOutline = true;

    private bool _isDead;
    private bool _inCinematic;
    private float _fade;
    private float _dissolve;
    private bool _isCounted;
    private Tween _fadeTween;

    private Color _outlineTint;
    private Color _fillTint;
    private Tween _colorTween;

    // Con Enter Play Mode sin domain reload el static sobreviviria entre sesiones de Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => ActiveCount = 0;

    private void Awake()
    {
        if (_player == null) _player = GetComponent<PlayerController>();

        // GetComponent y no _player.NodeHandler: esa propiedad se asigna en el Awake de
        // PlayerController y el orden de Awake/OnEnable entre componentes no esta garantizado.
        if (_nodeHandler == null) _nodeHandler = GetComponent<PlayerNodeHandler>();
    }

    private void OnEnable()
    {
        SetLayer(true);

        if (_player != null)
        {
            _player.OnDied += HandleDied;
            _player.OnRespawned += HandleRespawned;
            _player.OnDissolving += HandleDissolving;
            _player.OnCinematicSetupRequested += HandleCinematicSetup;
            _player.OnCinematicRestoreRequested += HandleCinematicRestore;
        }

        if (_nodeHandler != null)
            _nodeHandler.OnNodeGrabbed += HandleNodeGrabbed;

        _isDead = false;
        _inCinematic = false;
        _dissolve = 0f;
        RefreshFade();

        // Sin tween: si arranca con nodo no tiene que verse un frame con el color equivocado.
        var level = _nodeHandler != null ? _nodeHandler.CurrentLevel : GlitchState.Clean;
        GetPaletteTints(level, out _outlineTint, out _fillTint);
        ApplyTints();
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnDied -= HandleDied;
            _player.OnRespawned -= HandleRespawned;
            _player.OnDissolving -= HandleDissolving;
            _player.OnCinematicSetupRequested -= HandleCinematicSetup;
            _player.OnCinematicRestoreRequested -= HandleCinematicRestore;
        }

        if (_nodeHandler != null)
            _nodeHandler.OnNodeGrabbed -= HandleNodeGrabbed;

        _fadeTween.Stop();
        _fade = 0f;
        ApplyIntensity();

        // Peso 0: el shader vuelve al color del material y no queda un tinte pegado entre escenas.
        _colorTween.Stop();
        Shader.SetGlobalVector(XRayOutlineTintId, Vector4.zero);
        Shader.SetGlobalVector(XRayFillTintId, Vector4.zero);

        SetLayer(false);
    }

    // Llega al agarrar, al soltar (nivel Clean) y cuando cambia el nivel del nodo en mano.
    private void HandleNodeGrabbed(bool hasNode, GlitchState level)
    {
        GetPaletteTints(hasNode ? level : GlitchState.Clean, out var targetOutline, out var targetFill);

        var fromOutline = _outlineTint;
        var fromFill = _fillTint;

        _colorTween.Stop();
        _colorTween = Tween.Custom(this, 0f, 1f, _colorDuration, (self, t) =>
        {
            self._outlineTint = Color.Lerp(fromOutline, targetOutline, t);
            self._fillTint = Color.Lerp(fromFill, targetFill, t);
            self.ApplyTints();
        }, _colorEase);
    }

    // Alpha = 1: peso del reemplazo en el shader, no opacidad (esa la pone el material).
    private void GetPaletteTints(GlitchState level, out Color outline, out Color fill)
    {
        var palette = GlitchPalette.Default;

        outline = _useEmissionForOutline ? palette.EmissionFor(level) : palette.ColorFor(level);
        outline.a = 1f;

        fill = palette.ColorFor(level);
        fill.a = 1f;
    }

    // SetGlobalColor y no SetGlobalVector: convierte a espacio lineal igual que Material.SetColor,
    // asi el tono coincide con el del nodo, la mochila y el resto de consumidores de la paleta.
    private void ApplyTints()
    {
        Shader.SetGlobalColor(XRayOutlineTintId, _outlineTint);
        Shader.SetGlobalColor(XRayFillTintId, _fillTint);
    }

    private void HandleDied()
    {
        _isDead = true;
        RefreshFade();
    }

    private void HandleRespawned()
    {
        _isDead = false;
        _dissolve = 0f;
        RefreshFade();
    }

    // El timer va 0 -> 1 al desintegrarse y 1 -> 0 al reintegrarse: el efecto lo sigue al reves.
    private void HandleDissolving(float timer)
    {
        _dissolve = Mathf.Clamp01(timer);
        ApplyIntensity();
    }

    private void HandleCinematicSetup(Transform parent, LayerMask cinematicLayer, bool storeCCState)
    {
        _inCinematic = true;
        RefreshFade();
    }

    private void HandleCinematicRestore(LayerMask defaultLayer, bool restoreCCState)
    {
        _inCinematic = false;
        RefreshFade();
    }

    private void RefreshFade()
    {
        var target = _isDead || _inCinematic ? 0f : 1f;

        _fadeTween.Stop();
        _fadeTween = Tween.Custom(this, _fade, target, _fadeDuration, (self, v) =>
        {
            self._fade = v;
            self.ApplyIntensity();
        }, _fadeEase);
    }

    private void ApplyIntensity()
    {
        var intensity = _fade * (1f - _dissolve);
        Shader.SetGlobalFloat(XRayIntensityId, intensity);

        // Contar solo mientras se ve algo: muerto o en cinematica la feature no cuesta nada.
        var shouldCount = intensity > 0f;
        if (shouldCount == _isCounted) return;

        _isCounted = shouldCount;
        ActiveCount += shouldCount ? 1 : -1;
    }

    // Se agrega/quita el bit sin pisar el resto del mask (Default, Receive decals).
    private void SetLayer(bool enable)
    {
        if (_renderers == null) return;

        var mask = _xrayLayer.value;

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            r.renderingLayerMask = enable ? r.renderingLayerMask | mask : r.renderingLayerMask & ~mask;
        }
    }
}
