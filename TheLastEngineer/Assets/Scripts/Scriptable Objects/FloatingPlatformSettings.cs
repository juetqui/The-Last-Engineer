using UnityEngine;

/// <summary>
/// Tuning for <c>FloatingPlatform</c>. The platform applies it to its Rigidbody and ConfigurableJoint
/// on Awake, so the components never need hand-tuning in the Inspector.
/// Defaults are tuned for a ~4 x 0.3 x 4 m platform carrying the player.
/// </summary>
[CreateAssetMenu(fileName = "FloatingPlatformSettings", menuName = "Platform/Floating Platform Settings")]
public class FloatingPlatformSettings : ScriptableObject
{
    [Header("Body")]
    [Tooltip("Platform mass (kg). Heavier = reacts slower to the passenger.")]
    [SerializeField, Min(1f)] private float _platformMass = 120f;

    [Tooltip("Angular damping of the Rigidbody. Kills leftover wobble.")]
    [SerializeField, Min(0f)] private float _angularDamping = 3f;

    [Tooltip("Smooths the rendered tilt between physics steps.")]
    [SerializeField] private bool _interpolate = true;

    [Header("Tilt")]
    [Tooltip("Max tilt (degrees) on each horizontal axis. Above ~15 the CharacterController starts sliding.")]
    [SerializeField, Range(1f, 45f)] private float _maxTiltAngle = 12f;

    [Tooltip("Spring that pulls the platform back to level (N·m per radian). Lower = tilts more under the same weight.")]
    [SerializeField, Min(0f)] private float _levelingSpring = 6000f;

    [Tooltip("Damper of the leveling spring. Higher = less bounce, slower response.")]
    [SerializeField, Min(0f)] private float _levelingDamper = 700f;

    [Tooltip("Bounce when hitting the tilt limit (0 = dead stop).")]
    [SerializeField, Range(0f, 1f)] private float _limitBounciness = 0.1f;

    [Header("Passenger")]
    [Tooltip("Weight the passenger pushes with (kg). A CharacterController has no mass of its own.")]
    [SerializeField, Min(0f)] private float _passengerMass = 70f;

    [Tooltip("Seconds the weight takes to fully apply / release when stepping on / off.")]
    [SerializeField, Min(0f)] private float _loadRampTime = 0.15f;

    [Tooltip("Height (m) of the detection box above the top face.")]
    [SerializeField, Min(0.05f)] private float _detectionHeight = 0.5f;

    [Tooltip("Layers scanned for passengers. Only CharacterControllers count.")]
    [SerializeField] private LayerMask _passengerMask = ~0;

    public float PlatformMass => _platformMass;
    public float AngularDamping => _angularDamping;
    public bool Interpolate => _interpolate;
    public float MaxTiltAngle => _maxTiltAngle;
    public float LevelingSpring => _levelingSpring;
    public float LevelingDamper => _levelingDamper;
    public float LimitBounciness => _limitBounciness;
    public float PassengerMass => _passengerMass;
    public float LoadRampTime => _loadRampTime;
    public float DetectionHeight => _detectionHeight;
    public LayerMask PassengerMask => _passengerMask;
}
