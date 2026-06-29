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
    private Glitcheable _glitcheable;
    private ParticleSystem[] _particleSystem;
    
    public Action<bool> OnPlayerInRange;
    
    private enum PSType
    {
        Idle,
        Corrupted
    }
    
    private void Awake()
    {
        _glitcheable = GetComponentInParent<Glitcheable>();
        _particleSystem = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void Start()
    {
        _glitcheable.FSM.OnStateChanged += SetUpPSColor;
        _glitcheable.OnInteractionRejected += BouncePS;
        _glitcheable.OnPlayerInRange += SetUpPlayerInRange;
    }

    private void SetUpPlayerInRange(PlayerController player, bool entered)
    {
        if (entered) player.OnGlitcheableInArea += SetUpParticles;
        else
        {
            player.OnGlitcheableInArea -= SetUpParticles;
            SetUpParticles(null);
        }
    }

    private void SetUpParticles(Glitcheable glitcheable)
    {
        var newIsInRange = _glitcheable == glitcheable && glitcheable != null;

        if (newIsInRange == _isPlayerInRange) return;

        if (_glitcheable.IsCorrupted && psType != PSType.Corrupted
        || !_glitcheable.IsCorrupted && psType != PSType.Idle)
        {
            return;
        }

        _isPlayerInRange = newIsInRange;
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
        var condition = _glitcheable.IsCorrupted && psType == PSType.Corrupted || !_glitcheable.IsCorrupted && psType == PSType.Idle;

        if (condition)
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

        if (state == _glitcheable.DisState)
            SetUpParticles(null);
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
