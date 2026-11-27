using UnityEngine;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Detects the closest collider overlapping a box (local center and size) on this object and feeds its position to a
/// VFX Graph, so the particles placed on a mesh are pushed along their normals (VFXGraph_RepelObject).
/// Runs in Edit Mode too ([ExecuteAlways]) so the effect can be tuned without entering Play Mode.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(VisualEffect))]
public class VfxMeshRepeller : MonoBehaviour
{
    private const int MaxOverlaps = 8;
    private const float NoRepeller = 0f;
    private const float FullRepeller = 1f;
    private const float HalfFactor = 0.5f;
    // Caps the editor delta so a long pause between edit-mode ticks doesn't snap the fade.
    private const float MaxEditorDeltaTime = 0.1f;

    // Names of the exposed properties in the VFX Graph Blackboard. A property of type Position is
    // exposed as "<name>_position"; a plain Vector3 keeps its name, so both are accepted.
    private static readonly int RepelPositionId = Shader.PropertyToID("RepelPos");
    private static readonly int RepelPositionFieldId = Shader.PropertyToID("RepelPos_position");
    private static readonly int RepelWeightId = Shader.PropertyToID("RepelWeight");

    private int _repelPositionId;

    [Tooltip("Center of the detection box, in this object's local space.")]
    [SerializeField] private Vector3 _boxCenter = new Vector3(0f, 3f, 0f);
    [Tooltip("Size of the detection box, in this object's local space (scaled and rotated with it).")]
    [SerializeField] private Vector3 _boxSize = new Vector3(6f, 6f, 6f);
    [SerializeField] private LayerMask _repelerLayers = ~0;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;
    [Tooltip("Units of RepelWeight per second when a repeler enters or leaves the box.")]
    [SerializeField, Min(0f)] private float _weightFadeSpeed = 4f;

    private readonly Collider[] _overlaps = new Collider[MaxOverlaps];
    private VisualEffect _vfx;
    private bool _hasProperties;
    private float _weight;
    private float _lastEditorTime;

    private void OnEnable()
    {
        _vfx = GetComponent<VisualEffect>();
        _hasProperties = HasRequiredProperties();
        _weight = NoRepeller;
        _lastEditorTime = Time.realtimeSinceStartup;
    }

    private void OnDisable()
    {
        // Leave the particles at rest so the mesh shape is restored when the component is off.
        _weight = NoRepeller;
        if (_hasProperties && _vfx != null)
            _vfx.SetFloat(RepelWeightId, NoRepeller);
    }

    private void Update()
    {
        if (!_hasProperties || _vfx == null)
            return;

        Collider closest = FindClosestOverlap();
        float targetWeight = closest != null ? FullRepeller : NoRepeller;
        _weight = Mathf.MoveTowards(_weight, targetWeight, _weightFadeSpeed * GetDeltaTime());

        if (closest != null)
            _vfx.SetVector3(_repelPositionId, closest.bounds.center);
        _vfx.SetFloat(RepelWeightId, _weight);

        KeepEditorTicking(targetWeight);
    }

    private Collider FindClosestOverlap()
    {
        // Edit Mode doesn't sync moved transforms into the physics scene on its own.
        if (!Application.isPlaying)
            Physics.SyncTransforms();

        Vector3 center = transform.TransformPoint(_boxCenter);
        Vector3 halfExtents = Vector3.Scale(_boxSize, transform.lossyScale) * HalfFactor;
        halfExtents = new Vector3(Mathf.Abs(halfExtents.x), Mathf.Abs(halfExtents.y), Mathf.Abs(halfExtents.z));
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlaps, transform.rotation, _repelerLayers, _triggerInteraction);

        Collider closest = null;
        float bestSqrDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider hit = _overlaps[i];
            if (hit == null || hit.transform.IsChildOf(transform))
                continue;

            float sqrDistance = (hit.bounds.center - center).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                closest = hit;
            }
        }
        return closest;
    }

    private float GetDeltaTime()
    {
        if (Application.isPlaying)
            return Time.deltaTime;

        // Time.deltaTime is not meaningful between edit-mode updates.
        float now = Time.realtimeSinceStartup;
        float delta = Mathf.Min(now - _lastEditorTime, MaxEditorDeltaTime);
        _lastEditorTime = now;
        return delta;
    }

    private bool HasRequiredProperties()
    {
        if (_vfx == null || _vfx.visualEffectAsset == null)
            return false;

        bool hasPosition = TryResolvePositionId(out _repelPositionId);
        bool hasWeight = _vfx.HasFloat(RepelWeightId);
        if (!hasPosition)
            Debug.LogWarning($"{nameof(VfxMeshRepeller)} on '{name}': the VFX Graph has no exposed " +
                             "'RepelPos' property of type Position or Vector3.", this);
        if (!hasWeight)
            Debug.LogWarning($"{nameof(VfxMeshRepeller)} on '{name}': the VFX Graph has no exposed " +
                             "'RepelWeight' property of type float.", this);
        return hasPosition && hasWeight;
    }

    private bool TryResolvePositionId(out int id)
    {
        id = _vfx.HasVector3(RepelPositionFieldId) ? RepelPositionFieldId : RepelPositionId;
        return _vfx.HasVector3(id);
    }

    // In Edit Mode Update only runs when something changes; while the weight is still fading,
    // ask for another player-loop tick so the fade finishes without having to move anything.
    private void KeepEditorTicking(float targetWeight)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && !Mathf.Approximately(_weight, targetWeight))
            EditorApplication.QueuePlayerLoopUpdate();
#endif
    }

    private void OnValidate()
    {
        // Re-check after the asset or the component changes in the Inspector.
        if (isActiveAndEnabled)
        {
            _vfx = GetComponent<VisualEffect>();
            _hasProperties = HasRequiredProperties();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _weight > NoRepeller ? Color.red : Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_boxCenter, _boxSize);
    }
}
