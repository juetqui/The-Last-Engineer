using System;
using UnityEngine;
using PrimeTween;

public class GlitcheableOrbitController : MonoBehaviour
{
    [SerializeField] private PSType psType = PSType.Idle;

    [Header("PS Scale")]
    [SerializeField] private float scaleTime = 0.3f;
    [SerializeField] private float upScale = 1f;
    [SerializeField] private float downScale = 0.1f;

    [Header("Bounce Animation")]
    [SerializeField] private float interactionUpScale = 1.1f;
    [SerializeField] private float interactionDownScale = 0.9f;
    [SerializeField] private float bounceDuration = 0.45f;
    [SerializeField] private float bounceDelay = 0f;
    [SerializeField] private Ease bounceSettleEaseType = Ease.OutElastic;

    [Header("PS Ease Type")]
    [SerializeField] private Ease scaleEaseType = Ease.OutQuad;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private bool _isPlayerInRange;
    // Proximidad real, independiente de si este orbe es el que se ve: cuando cambia el nivel y le
    // toca a otro orbe, el nuevo tiene que saber si crecer sin esperar a que el jugador salga y entre.
    private bool _playerNear;
    private Glitcheable _glitcheable;
    private ParticleSystem[] _particleSystem;
    private ParticleSystem.MinMaxGradient[] _originalColors;
    private PlayerController _subscribedPlayer;

    // Si el prefab no trae un orbe propio para Intangible, el de Idle lo cubre teñido con la
    // paleta. Así el nivel 1 se distingue sin obligar a tocar cada prefab.
    private bool _hasIntangibleOrbit;

    public Action<bool> OnPlayerInRange;

    // Intangible va al final: el enum se serializa como int en los prefabs.
    private enum PSType
    {
        Idle,
        Corrupted,
        Intangible
    }

    private void Awake()
    {
        _glitcheable = GetComponentInParent<Glitcheable>();
        _particleSystem = GetComponentsInChildren<ParticleSystem>(true);

        _originalColors = new ParticleSystem.MinMaxGradient[_particleSystem.Length];
        for (int i = 0; i < _particleSystem.Length; i++)
            _originalColors[i] = _particleSystem[i].main.startColor;
    }

    private void Start()
    {
        // En Start y no en Awake: hace falta que todos los orbes hermanos ya estén despiertos.
        foreach (var orbit in _glitcheable.GetComponentsInChildren<GlitcheableOrbitController>(true))
            if (orbit.psType == PSType.Intangible) _hasIntangibleOrbit = true;

        _glitcheable.FSM.OnStateChanged += SetUpPSColor;
        _glitcheable.OnInteractionRejected += BouncePS;
        _glitcheable.OnPlayerInRange += SetUpPlayerInRange;
        _glitcheable.Glitch.OnGlitchStateChanged += OnLevelChanged;

        ApplyTint();
    }

    private void OnDestroy()
    {
        if (_glitcheable != null)
        {
            if (_glitcheable.FSM != null) _glitcheable.FSM.OnStateChanged -= SetUpPSColor;
            if (_glitcheable.Glitch != null) _glitcheable.Glitch.OnGlitchStateChanged -= OnLevelChanged;
            _glitcheable.OnInteractionRejected -= BouncePS;
            _glitcheable.OnPlayerInRange -= SetUpPlayerInRange;
        }

        if (_subscribedPlayer != null) _subscribedPlayer.OnGlitcheableInArea -= SetUpParticles;

        if (_particleSystem == null) return;

        foreach (var ps in _particleSystem)
        {
            if (ps != null)
                Tween.StopAll(onTarget: ps.transform);
        }
    }

    private PSType ActiveType()
    {
        if (_glitcheable.IsCorrupted) return PSType.Corrupted;
        if (_glitcheable.Level == GlitchState.Intangible && _hasIntangibleOrbit) return PSType.Intangible;

        return PSType.Idle;
    }

    private bool IsMyTurn() => psType == ActiveType();

    private void SetUpPlayerInRange(PlayerController player, bool entered)
    {
        if (entered)
        {
            if (_subscribedPlayer != null) _subscribedPlayer.OnGlitcheableInArea -= SetUpParticles;

            _subscribedPlayer = player;
            player.OnGlitcheableInArea += SetUpParticles;
        }
        else
        {
            player.OnGlitcheableInArea -= SetUpParticles;
            if (_subscribedPlayer == player) _subscribedPlayer = null;

            SetUpParticles(null);
        }
    }

    private void SetUpParticles(Glitcheable glitcheable)
    {
        _playerNear = _glitcheable == glitcheable && glitcheable != null;

        ApplyRange();
    }

    private void ApplyRange()
    {
        if (_playerNear == _isPlayerInRange) return;
        if (!IsMyTurn()) return;

        _isPlayerInRange = _playerNear;
        OnPlayerInRange?.Invoke(_isPlayerInRange);

        foreach (var ps in _particleSystem)
        {
            if (_isPlayerInRange)
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }
            else ps.Stop();

            var targetScale = _isPlayerInRange ? Vector3.one * upScale : Vector3.one * downScale;

            Tween.StopAll(onTarget: ps.transform);
            Tween.Scale(ps.transform, targetScale, scaleTime, scaleEaseType).OnComplete(() =>
            {
                if (!_isPlayerInRange) ps.gameObject.SetActive(false);
            });
        }
    }

    private void SetUpPSColor(IState state)
    {
        RefreshVisibility();

        if (state == _glitcheable.DisState)
            SetUpParticles(null);
    }

    // Clean <-> Intangible no cambia el estado de la FSM, así que sin esto el orbe no se enteraría.
    private void OnLevelChanged(GlitchState level)
    {
        RefreshVisibility();
        ApplyRange();
    }

    private void RefreshVisibility()
    {
        ApplyTint();

        if (IsMyTurn())
        {
            foreach (var ps in _particleSystem)
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }
        }
        else
        {
            foreach (var ps in _particleSystem)
            {
                ps.Stop();
                ps.gameObject.SetActive(false);
            }
        }

        if (debug) Debug.Log($"[GlitcheableOrbitController] {name} ({psType}) activo={IsMyTurn()} nivel={_glitcheable.Level}", this);
    }

    private void ApplyTint()
    {
        if (psType != PSType.Idle || _hasIntangibleOrbit) return;

        var tinted = _glitcheable.Level == GlitchState.Intangible;

        for (int i = 0; i < _particleSystem.Length; i++)
        {
            var main = _particleSystem[i].main;
            main.startColor = tinted
                ? new ParticleSystem.MinMaxGradient(GlitchPalette.Default.ColorFor(GlitchState.Intangible))
                : _originalColors[i];
        }
    }

    private void BouncePS()
    {
        foreach (var ps in _particleSystem)
        {
            var go = ps.gameObject;
            var baseScale = _isPlayerInRange ? Vector3.one * upScale : Vector3.one * downScale;
            var targetUp   = Vector3.one * interactionUpScale;
            var targetDown = Vector3.one * interactionDownScale;

            var phase1 = bounceDuration * 0.35f;
            var phase2 = bounceDuration * 0.30f;
            var phase3 = bounceDuration * 0.35f;

            Tween.StopAll(onTarget: go.transform);

            Tween.Scale(go.transform, targetUp, phase1, Ease.OutQuad, startDelay: bounceDelay)
                .OnComplete(() =>
                {
                    Tween.Scale(go.transform, targetDown, phase2, Ease.InOutQuad)
                        .OnComplete(() =>
                        {
                            Tween.Scale(go.transform, baseScale, phase3, bounceSettleEaseType);
                        });
                });
        }
    }
}
