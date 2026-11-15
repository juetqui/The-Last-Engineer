using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics platform that floats in place and tilts toward the passenger's weight, then levels
/// itself back. Tilt, limits and leveling come from a ConfigurableJoint anchored to the world; this
/// component only adds the weight, because a CharacterController never pushes a Rigidbody.
/// Not for route platforms: <c>PlatformController</c> forces its Rigidbody kinematic.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(ConfigurableJoint), typeof(BoxCollider))]
public class FloatingPlatform : MonoBehaviour
{
    private const int OverlapBufferSize = 8;

    [SerializeField] private FloatingPlatformSettings _settings;

    private readonly Collider[] _overlaps = new Collider[OverlapBufferSize];
    private readonly List<Collider> _passengerColliders = new List<Collider>();

    private Rigidbody _body;
    private ConfigurableJoint _joint;
    private BoxCollider _surface;
    private CharacterController _knownPassenger;
    private float _load;
    private Vector3 _localLoadPoint;

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _joint = GetComponent<ConfigurableJoint>();
        _surface = GetComponent<BoxCollider>();

        if (_settings == null)
        {
            Debug.LogError($"[FloatingPlatform] '{name}' has no FloatingPlatformSettings assigned; disabled.", this);
            enabled = false;
            return;
        }

        ApplySettings();
    }

    /// <summary>Re-applies the settings asset; use it in Play Mode after tweaking the asset.</summary>
    [ContextMenu("Apply Settings")]
    public void ApplySettings()
    {
        if (_settings == null || _body == null || _joint == null) return;

        ConfigureBody();
        ConfigureJoint();
    }

    private void FixedUpdate()
    {
        bool loaded = TryFindPassenger(out Vector3 localPoint);
        if (loaded) _localLoadPoint = localPoint;

        _load = FloatingPlatformLoad.StepLoad(_load, loaded, _settings.LoadRampTime, Time.fixedDeltaTime);
        if (_load <= 0f) return;

        Vector3 weight = Physics.gravity * (_settings.PassengerMass * _load);
        _body.AddForceAtPosition(weight, transform.TransformPoint(_localLoadPoint), ForceMode.Force);
    }

    private void ConfigureBody()
    {
        _body.isKinematic = false;
        _body.useGravity = false;
        _body.mass = _settings.PlatformMass;
        _body.angularDamping = _settings.AngularDamping;
        _body.interpolation = _settings.Interpolate ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
    }

    private void ConfigureJoint()
    {
        _joint.connectedBody = null;
        _joint.autoConfigureConnectedAnchor = true;
        _joint.anchor = _surface.center;
        _joint.axis = Vector3.right;
        _joint.secondaryAxis = Vector3.up;
        _joint.enableCollision = false;

        _joint.xMotion = ConfigurableJointMotion.Locked;
        _joint.yMotion = ConfigurableJointMotion.Locked;
        _joint.zMotion = ConfigurableJointMotion.Locked;

        // Joint X = local right, Y (secondary) = local up, Z = local forward: tilt on X and Z, no yaw.
        _joint.angularXMotion = ConfigurableJointMotion.Limited;
        _joint.angularYMotion = ConfigurableJointMotion.Locked;
        _joint.angularZMotion = ConfigurableJointMotion.Limited;

        float maxTilt = _settings.MaxTiltAngle;
        float bounciness = _settings.LimitBounciness;
        _joint.lowAngularXLimit = new SoftJointLimit { limit = -maxTilt, bounciness = bounciness };
        _joint.highAngularXLimit = new SoftJointLimit { limit = maxTilt, bounciness = bounciness };
        _joint.angularZLimit = new SoftJointLimit { limit = maxTilt, bounciness = bounciness };

        var leveling = new JointDrive
        {
            positionSpring = _settings.LevelingSpring,
            positionDamper = _settings.LevelingDamper,
            maximumForce = float.MaxValue
        };
        _joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        _joint.angularXDrive = leveling;
        _joint.angularYZDrive = leveling;
        _joint.targetRotation = Quaternion.identity;
    }

    private bool TryFindPassenger(out Vector3 localPoint)
    {
        localPoint = default;

        Vector3 size = _surface.size;
        Vector3 scale = transform.lossyScale;
        float detectionHeight = _settings.DetectionHeight;

        Vector3 topCenter = transform.TransformPoint(_surface.center + Vector3.up * (size.y * 0.5f));
        Vector3 center = topCenter + transform.up * (detectionHeight * 0.5f);
        var halfExtents = new Vector3(
            Mathf.Abs(size.x * scale.x) * 0.5f,
            detectionHeight * 0.5f,
            Mathf.Abs(size.z * scale.z) * 0.5f);

        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlaps, transform.rotation,
            _settings.PassengerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            // Type check, not GetComponent: the player's CapsuleCollider shares the GameObject and would count twice.
            if (_overlaps[i] is not CharacterController passenger) continue;

            IgnorePassengerExtraColliders(passenger);

            Vector3 feet = passenger.bounds.center - Vector3.up * passenger.bounds.extents.y;
            localPoint = FloatingPlatformLoad.ClampToTopFace(transform.InverseTransformPoint(feet), _surface.center, size);
            return true;
        }

        return false;
    }

    // The player's root CapsuleCollider has no Rigidbody (a static collider that moves): touching this
    // dynamic body it would shove the platform. Only the CharacterController should stand on it.
    private void IgnorePassengerExtraColliders(CharacterController passenger)
    {
        if (passenger == _knownPassenger) return;
        _knownPassenger = passenger;

        passenger.GetComponents(_passengerColliders);
        foreach (Collider other in _passengerColliders)
        {
            if (other == passenger || other.isTrigger) continue;
            Physics.IgnoreCollision(_surface, other, true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_settings == null || !TryGetComponent(out BoxCollider surface)) return;

        // The detection height is in world meters; convert it to local units for the local matrix.
        float localHeight = _settings.DetectionHeight / Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Epsilon);
        Vector3 size = surface.size;
        Vector3 localCenter = surface.center + Vector3.up * ((size.y + localHeight) * 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(localCenter, new Vector3(size.x, localHeight, size.z));
    }
}
