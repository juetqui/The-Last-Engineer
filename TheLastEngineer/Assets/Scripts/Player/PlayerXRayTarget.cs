using PrimeTween;
using UnityEngine;

/// <summary>
/// Marca los renderers del jugador con la Rendering Layer que dibuja PlayerXRayFeature (outline +
/// relleno cuando una pared lo tapa) y controla la intensidad global del efecto.
///
/// El efecto se apaga con fade al morir y durante cinematicas, y se desvanece junto con el dissolve
/// del teleport: el override material de la mascara no tiene el alpha clip del dissolve, asi que
/// sin esto se veria la silueta entera mientras el jugador se desintegra.
/// </summary>
public class PlayerXRayTarget : MonoBehaviour
{
    private static readonly int XRayIntensityId = Shader.PropertyToID("_XRayIntensity");

    /// <summary>Targets con intensidad > 0. Con 0 la feature no encola ningun pass.</summary>
    public static int ActiveCount { get; private set; }

    [SerializeField] private PlayerController _player;
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private RenderingLayerMask _xrayLayer;

    [Header("Fade")]
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private Ease _fadeEase = Ease.OutQuad;

    private bool _isDead;
    private bool _inCinematic;
    private float _fade;
    private float _dissolve;
    private bool _isCounted;
    private Tween _fadeTween;

    // Con Enter Play Mode sin domain reload el static sobreviviria entre sesiones de Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => ActiveCount = 0;

    private void Awake()
    {
        if (_player == null) _player = GetComponent<PlayerController>();
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

        _isDead = false;
        _inCinematic = false;
        _dissolve = 0f;
        RefreshFade();
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

        _fadeTween.Stop();
        _fade = 0f;
        ApplyIntensity();
        SetLayer(false);
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
