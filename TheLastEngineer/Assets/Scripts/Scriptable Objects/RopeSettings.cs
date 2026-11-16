using UnityEngine;

/// <summary>
/// Tuning for <c>Rope</c>. The rope builds and configures every segment (Rigidbody, CapsuleCollider,
/// ConfigurableJoint) from this asset on Awake. Defaults suit a 3-8 m rope hanging a heavy platform.
/// </summary>
[CreateAssetMenu(fileName = "RopeSettings", menuName = "Platform/Rope Settings")]
public class RopeSettings : ScriptableObject
{
    [Header("Shape")]
    [Tooltip("Target length of each segment (m). Shorter = smoother but heavier and less stable.")]
    [SerializeField, Min(0.05f)] private float _segmentLength = 0.4f;

    [Tooltip("Collider radius of each segment (m). Also the LineRenderer half-width.")]
    [SerializeField, Min(0.005f)] private float _radius = 0.04f;

    [Tooltip("Extra length over the distance between ends (0.08 = 8% longer). 0 = taut: it pulls its ends constantly.")]
    [SerializeField, Range(0f, 1f)] private float _slack = 0.08f;

    [Tooltip("Hard cap on segments, whatever the distance.")]
    [SerializeField, Range(1, 128)] private int _maxSegments = 48;

    [Header("Segment body")]
    [Tooltip("Mass of each segment (kg).")]
    [SerializeField, Min(0.01f)] private float _segmentMass = 1.5f;

    [SerializeField, Min(0f)] private float _linearDamping = 0.1f;

    [SerializeField, Min(0f)] private float _angularDamping = 0.5f;

    [Tooltip("Smooths the rendered rope between physics steps.")]
    [SerializeField] private bool _interpolate = true;

    [Tooltip("Per-segment solver iterations (Unity default 6). Higher = less stretch, more CPU.")]
    [SerializeField, Range(1, 64)] private int _solverIterations = 12;

    [Tooltip("Per-segment solver velocity iterations (Unity default 1).")]
    [SerializeField, Range(1, 64)] private int _solverVelocityIterations = 4;

    [Header("Ends")]
    [Tooltip("Connected Mass Scale on the joints to each end's Rigidbody. A 120 kg platform on a 1.5 kg segment jitters; below 1 the joint sees the end as lighter.")]
    [SerializeField, Range(0.01f, 1f)] private float _endConnectedMassScale = 0.1f;

    [Header("Segment prefab (visual)")]
    [Tooltip("Instantiated as every segment; the rope adds Rigidbody, CapsuleCollider and joints to it. Visual only: model along local Y, centred on the root pivot; its root rotation is replaced. Empty = invisible segments (use RopeLine).")]
    [SerializeField] private GameObject _linkPrefab;

    [Tooltip("Length of the prefab along its local Y at scale 1 (m). Used to stretch it to each segment.")]
    [SerializeField, Min(0.001f)] private float _linkLength = 0.4f;

    [Tooltip("Scale the link along Y so it exactly covers its segment (segments vary slightly in length).")]
    [SerializeField] private bool _stretchLinkToSegment = true;

    [Tooltip("Twist added around the rope axis on each link (90 = alternating chain links, 0 = all aligned).")]
    [SerializeField, Range(0f, 180f)] private float _linkTwist = 90f;

    public GameObject LinkPrefab => _linkPrefab;
    public float LinkLength => _linkLength;
    public bool StretchLinkToSegment => _stretchLinkToSegment;
    public float LinkTwist => _linkTwist;

    public float SegmentLength => _segmentLength;
    public float Radius => _radius;
    public float Slack => _slack;
    public int MaxSegments => _maxSegments;
    public float SegmentMass => _segmentMass;
    public float LinearDamping => _linearDamping;
    public float AngularDamping => _angularDamping;
    public bool Interpolate => _interpolate;
    public int SolverIterations => _solverIterations;
    public int SolverVelocityIterations => _solverVelocityIterations;
    public float EndConnectedMassScale => _endConnectedMassScale;
}
