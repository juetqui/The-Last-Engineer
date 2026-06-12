using UnityEngine;

public class GlitcheableOrbitAnimator : MonoBehaviour
{
    [Header("Orbit Axes")]
    [Tooltip("Primary rotation axis in local space (e.g. Vector3.up for the equatorial ring).")]
    [SerializeField] private Vector3 primaryAxis = Vector3.up;
    [SerializeField] private float primarySpeed = 60f;

    [Tooltip("Optional secondary axis for a tilted-ring astrolabe effect. Set speed to 0 to disable.")]
    [SerializeField] private Vector3 secondaryAxis = Vector3.right;
    [SerializeField] private float secondarySpeed;

    [Header("Smooth Fade")]
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private LeanTweenType fadeEase = LeanTweenType.easeOutQuad;
    
    [SerializeField] private bool debug = false;

    private GlitcheableOrbitController _orbitController;
    private LTDescr _fadeTween;

    private float _speedMultiplier;

    private void Awake()
    {
        _orbitController = GetComponentInParent<GlitcheableOrbitController>();
        _orbitController.OnPlayerInRange += SetUpAnimation;
    }

    private void OnDestroy()
    {
        if (_orbitController != null)
            _orbitController.OnPlayerInRange -= SetUpAnimation;

        if (_fadeTween != null)
            LeanTween.cancel(_fadeTween.uniqueId);
    }

    private void Update()
    {
        if (_speedMultiplier <= 0f) return;

        OrbitObjectRotation();
    }

    private void SetUpAnimation(bool isPlayerInRange)
    {
        var target = isPlayerInRange ? 1f : 0f;

        if (_fadeTween != null)
            LeanTween.cancel(_fadeTween.uniqueId);

        _fadeTween = LeanTween.value(_orbitController.gameObject, _speedMultiplier, target, fadeTime)
                              .setEase(fadeEase)
                              .setOnUpdate(OnSpeedMultiplierUpdated);
    }

    private void OnSpeedMultiplierUpdated(float value) => _speedMultiplier = value;

    private void OrbitObjectRotation()
    {
        var dt = Time.deltaTime * _speedMultiplier;

        if (primarySpeed != 0f)
            transform.Rotate(primaryAxis, primarySpeed * dt, Space.Self);

        if (secondarySpeed != 0f)
            transform.Rotate(secondaryAxis, secondarySpeed * dt, Space.Self);
    }
}
