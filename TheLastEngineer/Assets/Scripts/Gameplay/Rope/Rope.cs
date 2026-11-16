using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a physics rope between two points on Awake: a chain of capsule segments linked by
/// ConfigurableJoints (position locked, rotation free), all tuned from <see cref="RopeSettings"/>.
/// Each segment is an instance of the settings' segment prefab (the visual) with the physics added on top.
/// Each end hooks to the first Rigidbody on its Transform or parents (e.g. a FloatingPlatform corner);
/// with none, that end is pinned to the world. Keep this object unscaled: segments are its children.
/// </summary>
[DisallowMultipleComponent]
public class Rope : MonoBehaviour
{
    private const string SegmentNamePrefix = "RopeSegment_";

    [SerializeField] private RopeSettings _settings;

    [Tooltip("Upper end of the rope.")]
    [SerializeField] private Transform _start;

    [Tooltip("Lower end of the rope (e.g. an empty at the platform corner).")]
    [SerializeField] private Transform _end;

    private readonly List<Rigidbody> _segments = new List<Rigidbody>();
    private readonly List<CapsuleCollider> _colliders = new List<CapsuleCollider>();
    private readonly List<float> _halfLengths = new List<float>();
    private readonly List<Joint> _strippedJoints = new List<Joint>();
    private readonly List<Rigidbody> _strippedBodies = new List<Rigidbody>();
    private readonly List<Collider> _strippedColliders = new List<Collider>();
    private bool _warnedAboutPrefabPhysics;
    private Vector3[] _gizmoPoints;

    /// <summary>Rope points from start to end: segment joints plus both ends. 0 before it is built.</summary>
    public int PointCount => _segments.Count == 0 ? 0 : _segments.Count + 1;

    public float Diameter => _settings != null ? _settings.Radius * 2f : 0f;

    public RopeSettings Settings => _settings;

    /// <summary>Built segments, start to end. 0 before Awake or when the rope failed to build.</summary>
    public int SegmentCount => _segments.Count;

    public Transform GetSegment(int index) => _segments[index].transform;

    public float GetSegmentLength(int index) => _halfLengths[index] * 2f;

    public Vector3 GetPoint(int index)
    {
        bool isStart = index == 0;
        int segment = isStart ? 0 : index - 1;
        Transform segmentTransform = _segments[segment].transform;

        // Rotation * half length, not TransformPoint: the segment's scale must not stretch the point.
        Vector3 toEnd = segmentTransform.rotation * Vector3.up * _halfLengths[segment];
        return isStart ? segmentTransform.position + toEnd : segmentTransform.position - toEnd;
    }

    private void Awake()
    {
        if (!IsValid())
        {
            enabled = false;
            return;
        }

        Build();
    }

    private bool IsValid()
    {
        if (_settings == null || _start == null || _end == null)
        {
            Debug.LogError($"[Rope] '{name}' needs Settings, Start and End assigned; not built.", this);
            return false;
        }

        if (Vector3.Distance(_start.position, _end.position) <= Mathf.Epsilon)
        {
            Debug.LogError($"[Rope] '{name}' Start and End are at the same position; not built.", this);
            return false;
        }

        if (transform.lossyScale != Vector3.one)
            Debug.LogWarning($"[Rope] '{name}' is scaled {transform.lossyScale}; segments are its children and will be distorted. Keep it at scale 1.", this);

        return true;
    }

    private void Build()
    {
        Vector3[] points = ComputeLayout(_start.position, _end.position);
        Rigidbody startBody = _start.GetComponentInParent<Rigidbody>();
        Rigidbody endBody = _end.GetComponentInParent<Rigidbody>();

        Rigidbody previous = startBody;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Rigidbody segment = CreateSegment(i, points[i], points[i + 1]);
            AddJoint(segment, previous, Vector3.up * LocalHalfLength(i), previous == startBody);
            previous = segment;
        }

        int lastIndex = _segments.Count - 1;
        AddJoint(_segments[lastIndex], endBody, Vector3.down * LocalHalfLength(lastIndex), true);

        IgnoreSelfCollisions();
        IgnoreCollidersAtEnd(_colliders[0], points[0]);
        IgnoreCollidersAtEnd(_colliders[lastIndex], points[points.Length - 1]);
    }

    private Vector3[] ComputeLayout(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float ropeLength = RopeLayout.RopeLength(distance, _settings.Slack);
        int count = RopeLayout.SegmentCount(ropeLength, _settings.SegmentLength, _settings.MaxSegments);

        var points = new Vector3[count + 1];
        RopeLayout.FillPoints(start, end, RopeLayout.SagDepth(distance, ropeLength), Physics.gravity, points);
        return points;
    }

    private Rigidbody CreateSegment(int index, Vector3 top, Vector3 bottom)
    {
        Vector3 axis = top - bottom;
        float length = axis.magnitude;

        GameObject segmentObject = InstantiateSegmentObject(index);
        Transform segmentTransform = segmentObject.transform;
        Quaternion twist = Quaternion.AngleAxis(_settings.LinkTwist * index, Vector3.up);
        segmentTransform.SetPositionAndRotation((top + bottom) * 0.5f, Quaternion.FromToRotation(Vector3.up, axis) * twist);
        StretchToSegment(segmentTransform, length);

        // Collider sizes are local: undo the segment's scale so the physics keep the settings' meters.
        Vector3 scale = segmentTransform.lossyScale;
        var capsule = segmentObject.AddComponent<CapsuleCollider>();
        capsule.direction = 1;
        capsule.radius = _settings.Radius / Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z), Mathf.Epsilon);
        capsule.height = length / Mathf.Max(Mathf.Abs(scale.y), Mathf.Epsilon);

        var body = segmentObject.AddComponent<Rigidbody>();
        body.mass = _settings.SegmentMass;
        body.linearDamping = _settings.LinearDamping;
        body.angularDamping = _settings.AngularDamping;
        body.interpolation = _settings.Interpolate ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.solverIterations = _settings.SolverIterations;
        body.solverVelocityIterations = _settings.SolverVelocityIterations;

        _segments.Add(body);
        _colliders.Add(capsule);
        _halfLengths.Add(length * 0.5f);
        return body;
    }

    // The segment prefab is the visual; with none the segment is an empty object (draw it with RopeLine).
    private GameObject InstantiateSegmentObject(int index)
    {
        GameObject prefab = _settings.LinkPrefab;
        GameObject segmentObject = prefab != null ? Instantiate(prefab, transform, false) : new GameObject();
        segmentObject.name = SegmentNamePrefix + index;
        segmentObject.layer = gameObject.layer;

        if (prefab != null) StripPhysics(segmentObject);
        return segmentObject;
    }

    private void StretchToSegment(Transform segmentTransform, float length)
    {
        GameObject prefab = _settings.LinkPrefab;
        if (prefab == null || !_settings.StretchLinkToSegment) return;

        Vector3 scale = prefab.transform.localScale;
        scale.y *= length / _settings.LinkLength;
        segmentTransform.localScale = scale;
    }

    // The rope adds its own Rigidbody, collider and joints; any the prefab brings would fight them.
    private void StripPhysics(GameObject segmentObject)
    {
        segmentObject.GetComponentsInChildren(true, _strippedJoints);
        segmentObject.GetComponentsInChildren(true, _strippedBodies);
        segmentObject.GetComponentsInChildren(true, _strippedColliders);
        if (_strippedJoints.Count == 0 && _strippedBodies.Count == 0 && _strippedColliders.Count == 0) return;

        if (!_warnedAboutPrefabPhysics)
        {
            _warnedAboutPrefabPhysics = true;
            Debug.LogWarning($"[Rope] Segment prefab of '{name}' has Joints/Rigidbodies/Colliders; removed from the segments. Remove them from the prefab.", this);
        }

        foreach (Joint joint in _strippedJoints) DestroyImmediate(joint);
        foreach (Rigidbody body in _strippedBodies) DestroyImmediate(body);
        foreach (Collider collider in _strippedColliders) DestroyImmediate(collider);
    }

    private float LocalHalfLength(int index) =>
        _halfLengths[index] / Mathf.Max(Mathf.Abs(_segments[index].transform.lossyScale.y), Mathf.Epsilon);

    private void AddJoint(Rigidbody segment, Rigidbody connected, Vector3 localAnchor, bool isEnd)
    {
        var joint = segment.gameObject.AddComponent<ConfigurableJoint>();
        joint.anchor = localAnchor;
        joint.connectedBody = connected;
        joint.autoConfigureConnectedAnchor = true;
        joint.enableCollision = false;
        // Preprocessing makes long joint chains jitter.
        joint.enablePreprocessing = false;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        if (isEnd && connected != null) joint.connectedMassScale = _settings.EndConnectedMassScale;
    }

    // Jointed neighbours already skip each other; this also stops non-adjacent links snagging.
    private void IgnoreSelfCollisions()
    {
        for (int i = 0; i < _colliders.Count; i++)
            for (int j = i + 1; j < _colliders.Count; j++)
                Physics.IgnoreCollision(_colliders[i], _colliders[j], true);
    }

    // An end segment starts touching whatever it hangs from (ceiling, platform corner): without this,
    // that contact shoves the segment out on the first step.
    private void IgnoreCollidersAtEnd(CapsuleCollider endSegment, Vector3 endPoint)
    {
        foreach (Collider other in Physics.OverlapSphere(endPoint, _settings.Radius * 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (_colliders.Contains(other as CapsuleCollider)) continue;
            Physics.IgnoreCollision(endSegment, other, true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying || _settings == null || _start == null || _end == null) return;
        if (Vector3.Distance(_start.position, _end.position) <= Mathf.Epsilon) return;

        float distance = Vector3.Distance(_start.position, _end.position);
        float ropeLength = RopeLayout.RopeLength(distance, _settings.Slack);
        int count = RopeLayout.SegmentCount(ropeLength, _settings.SegmentLength, _settings.MaxSegments);
        if (_gizmoPoints == null || _gizmoPoints.Length != count + 1) _gizmoPoints = new Vector3[count + 1];

        RopeLayout.FillPoints(_start.position, _end.position, RopeLayout.SagDepth(distance, ropeLength), Physics.gravity, _gizmoPoints);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < count; i++)
        {
            Gizmos.DrawLine(_gizmoPoints[i], _gizmoPoints[i + 1]);
            Gizmos.DrawWireSphere(_gizmoPoints[i], _settings.Radius);
        }
        Gizmos.DrawWireSphere(_gizmoPoints[count], _settings.Radius);
    }
}
