using System;
using UnityEngine;

public class GlitcheableOrbitController : MonoBehaviour
{
    [Header("PS State Color")]
    [SerializeField] private Gradient corruptedColor;
    [SerializeField] private Gradient idleColor;
    
    [Header("PS Scale")]
    [SerializeField] private float scaleTime = 0.3f;
    [SerializeField] private float upScale = 1f;
    [SerializeField] private float downScale = 0.1f;

    [Header("Bounce Animation")]
    [SerializeField] private float interactionUpScale = 1.1f;
    [SerializeField] private float interactionDownScale = 0.9f;
    [SerializeField] private float bounceDuration = 0.45f;
    [SerializeField] private float bounceDelay = 0f;
    [SerializeField] private LeanTweenType bounceSettleEaseType = LeanTweenType.easeOutElastic;
    
    [Header("PS Ease Type")]
    [SerializeField] private LeanTweenType scaleEaseType = LeanTweenType.easeOutQuad;
    
    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private bool _isPlayerInRange;
    private Glitcheable _glitcheable;
    private ParticleSystem[] _particleSystem;
    private ParticleSystem.Particle[][] _particleBuffers;
    
    public Action<bool> OnPlayerInRange;
    
    private void Awake()
    {
        _glitcheable = GetComponentInParent<Glitcheable>();
        _particleSystem = GetComponentsInChildren<ParticleSystem>(true);

        _particleBuffers = new ParticleSystem.Particle[_particleSystem.Length][];
        for (int i = 0; i < _particleSystem.Length; i++)
            _particleBuffers[i] = new ParticleSystem.Particle[_particleSystem[i].main.maxParticles];

        _glitcheable.FSM.OnStateChanged += SetUpPSColor;
        _glitcheable.OnInteractionRejected += BouncePS;
    }

    private void Start()
    {
        PlayerController.Instance.OnGlitcheableInArea += SetUpParticles;
    }

    private void SetUpParticles(Glitcheable glitcheable)
    {
        var newIsInRange = _glitcheable == glitcheable && glitcheable != null;

        if (newIsInRange == _isPlayerInRange) return;

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

            LeanTween.cancel(ps.gameObject);
            LeanTween.scale(ps.gameObject, targetScale, scaleTime).setEase(scaleEaseType).setOnComplete(() =>
            {
                if (!_isPlayerInRange) ps.gameObject.SetActive(false);
            });
        }
    }

    private void SetUpPSColor(IState state)
    {
        if (state == _glitcheable.DisState)
        {
            SetUpParticles(null);
            return;
        }

        var gradient = _glitcheable.IsCorrupted ? corruptedColor : idleColor;

        for (int i = 0; i < _particleSystem.Length; i++)
        {
            var ps = _particleSystem[i];
            var buffer = _particleBuffers[i];

            // Update colorOverLifetime — Unity re-evaluates this per-frame for all alive particles
            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = new ParticleSystem.MinMaxGradient(gradient);

            // Ensure new particles start white so colorOverLifetime is the sole color driver
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(gradient);

            // Set startColor of each alive particle to the gradient color at its current normalized age
            int count = ps.GetParticles(buffer);

            for (int j = 0; j < count; j++)
            {
                float normalizedAge = 1f - buffer[j].remainingLifetime / buffer[j].startLifetime;
                buffer[j].startColor = gradient.Evaluate(normalizedAge);
            }

            ps.SetParticles(buffer, count);
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

            LeanTween.cancel(go);

            LeanTween.scale(go, targetUp, phase1)
                .setEase(LeanTweenType.easeOutQuad)
                .setDelay(bounceDelay)
                .setOnComplete(() =>
                {
                    LeanTween.scale(go, targetDown, phase2)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnComplete(() =>
                        {
                            LeanTween.scale(go, baseScale, phase3)
                                .setEase(bounceSettleEaseType);
                        });
                });
        }
    }
}
