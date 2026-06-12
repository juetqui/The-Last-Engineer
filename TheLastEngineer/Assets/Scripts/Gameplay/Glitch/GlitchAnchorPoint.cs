using UnityEngine;

/// <summary>
/// Autonomous glitch target point. When parented under a PlatformController,
/// it subscribes to its direction-change event and repositions itself so that
/// its offset relative to the platform's travel direction stays coherent after
/// a reversal. If no PlatformController is found in the hierarchy the component
/// is inert and the Transform is used as-is.
/// </summary>
[DefaultExecutionOrder(-2)]
public class GlitchAnchorPoint : MonoBehaviour
{
    // Offset components expressed in the platform's "travel-direction basis":
    //   forward  = along the platform's current travel direction
    //   up       = world up
    //   lateral  = cross(up, forward)
    private float _forwardOffset;
    private float _upOffset;
    private float _lateralOffset;

    private PlatformController _platform;
    private bool _hasPlatform;

    private void Start()
    {
        _platform = GetComponentInParent<PlatformController>();

        if (_platform == null)
        {
            _hasPlatform = false;
            return;
        }

        Vector3 initialDir = _platform.InitialDirection;

        // If the route has fewer than 2 points the direction is undefined;
        // fall back to treating the anchor as a static point.
        if (initialDir.sqrMagnitude < 0.001f)
        {
            _hasPlatform = false;
            return;
        }

        _hasPlatform = true;

        // Decompose current world offset from platform into (forward, up, lateral).
        Vector3 worldOffset = transform.position - _platform.transform.position;
        Vector3 lateral = Vector3.Cross(Vector3.up, initialDir).normalized;

        _forwardOffset  = Vector3.Dot(worldOffset, initialDir);
        _upOffset       = Vector3.Dot(worldOffset, Vector3.up);
        _lateralOffset  = Vector3.Dot(worldOffset, lateral);

        _platform.OnDirectionChanged += Reorient;
    }

    private void OnDestroy()
    {
        if (_hasPlatform && _platform != null)
            _platform.OnDirectionChanged -= Reorient;
    }

    /// <summary>
    /// Called by PlatformController whenever the platform's travel direction
    /// changes. Recomputes this anchor's world position using the new direction
    /// basis so that the forward/lateral offsets remain semantically correct.
    /// </summary>
    private void Reorient(Vector3 newDir)
    {
        Vector3 newLateral = Vector3.Cross(Vector3.up, newDir).normalized;

        transform.position = _platform.transform.position
            + newDir      * _forwardOffset
            + Vector3.up  * _upOffset
            + newLateral  * _lateralOffset;
    }
}

