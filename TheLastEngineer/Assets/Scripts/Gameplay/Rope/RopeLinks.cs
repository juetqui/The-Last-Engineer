using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws a <see cref="Rope"/> by placing one instance of the settings' link prefab on every segment,
/// parented to it so it follows the physics. Alternative to <see cref="RopeLine"/>; use one or the other.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rope))]
public class RopeLinks : MonoBehaviour
{
    private const string LinkNamePrefix = "RopeLink_";

    private readonly List<Collider> _strippedColliders = new List<Collider>();
    private readonly List<Rigidbody> _strippedBodies = new List<Rigidbody>();

    // Start, not Awake: the Rope builds its segments in its own Awake.
    private void Start()
    {
        var rope = GetComponent<Rope>();
        RopeSettings settings = rope.Settings;

        if (settings == null || settings.LinkPrefab == null)
        {
            Debug.LogError($"[RopeLinks] '{name}' has no link prefab in its RopeSettings; nothing drawn.", this);
            return;
        }

        for (int i = 0; i < rope.SegmentCount; i++)
            SpawnLink(settings, rope.GetSegment(i), rope.GetSegmentLength(i), i);
    }

    private void SpawnLink(RopeSettings settings, Transform segment, float segmentLength, int index)
    {
        GameObject link = Instantiate(settings.LinkPrefab, segment, false);
        link.name = LinkNamePrefix + index;
        link.transform.localPosition = Vector3.zero;
        link.transform.localRotation = Quaternion.AngleAxis(settings.LinkTwist * index, Vector3.up) * settings.LinkPrefab.transform.localRotation;

        if (settings.StretchLinkToSegment)
        {
            Vector3 scale = settings.LinkPrefab.transform.localScale;
            scale.y *= segmentLength / settings.LinkLength;
            link.transform.localScale = scale;
        }

        StripPhysics(link);
    }

    // A collider under the segment would join its compound collider and change its mass and contacts.
    private void StripPhysics(GameObject link)
    {
        link.GetComponentsInChildren(true, _strippedBodies);
        link.GetComponentsInChildren(true, _strippedColliders);
        if (_strippedBodies.Count == 0 && _strippedColliders.Count == 0) return;

        Debug.LogWarning($"[RopeLinks] Link prefab on '{name}' has Rigidbodies/Colliders; they were removed from the instances. Remove them from the prefab.", this);

        foreach (Rigidbody body in _strippedBodies) Destroy(body);
        foreach (Collider collider in _strippedColliders)
        {
            // Destroy is deferred to the end of the frame; disabling takes it out of physics now.
            collider.enabled = false;
            Destroy(collider);
        }
    }
}
