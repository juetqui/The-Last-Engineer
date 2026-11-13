using UnityEngine;

/// <summary>
/// Moving platforms and the elevator need a kinematic Rigidbody so moving them doesn't move a static
/// collider. Prefabs carry it; this covers scene objects and older scenes that don't.
/// </summary>
public static class KinematicBody
{
    public static Rigidbody Ensure(GameObject owner)
    {
        if (!owner.TryGetComponent(out Rigidbody body))
        {
            body = owner.AddComponent<Rigidbody>();
            Debug.LogWarning($"[KinematicBody] '{owner.name}' had no Rigidbody; added a kinematic one at runtime. Add it to the prefab or scene object.", owner);
        }
        else if (!body.isKinematic || body.useGravity || body.interpolation != RigidbodyInterpolation.None)
        {
            Debug.LogWarning($"[KinematicBody] '{owner.name}' Rigidbody must be kinematic, without gravity or interpolation; fixed at runtime.", owner);
        }

        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.None;
        return body;
    }
}
