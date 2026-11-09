using System;
using UnityEngine;

/// <summary>
/// Single holder of an object's glitch state (a NodeController, a Glitcheable or an Intangible).
/// Reactive behaviour (visuals, colliders, FSMs) subscribes to OnGlitchStateChanged instead of
/// being called by hand. Only GlitchTransferManager changes the state.
/// </summary>
[DisallowMultipleComponent]
public class GlitchComponent : MonoBehaviour
{
    // The state IS the serialized field, with no init step: it is valid right after
    // deserialization. Glitcheable runs with DefaultExecutionOrder(-1) and would read a default
    // value from any init done in a normal-order Awake. Name and type kept: prefabs store it as int.
    [SerializeField] private GlitchState _currentLevel = GlitchState.Clean;

    public GlitchState CurrentState => _currentLevel;

    /// <summary>
    /// Only raised on changes, NEVER in Awake/Start: raising the initial state would race with the
    /// subscribers' init order. Consumers read the initial state by pull (CurrentState) in Start.
    /// </summary>
    public event Action<GlitchState> OnGlitchStateChanged;

    /// <summary>
    /// Safety net for a prefab that forgot the component: adds it at runtime starting Clean and
    /// warns. The authored component on the prefab is the only place to pick the initial state.
    /// </summary>
    public static GlitchComponent Ensure(GameObject go)
    {
        var glitch = go.GetComponent<GlitchComponent>();
        if (glitch != null) return glitch;

        Debug.LogWarning($"[GlitchComponent] {go.name} has no GlitchComponent: adding one at runtime " +
                         "starting Clean. Add it to the prefab to pick the initial state.", go);

        return go.AddComponent<GlitchComponent>();
    }

    /// <summary>What owns this component; the Editor uses it to filter the states it can pick.</summary>
    public GlitchContainerKind? ResolveKind()
    {
        if (TryGetComponent<Glitcheable>(out _)) return GlitchContainerKind.Glitcheable;
        if (TryGetComponent<Intangible>(out _)) return GlitchContainerKind.Intangible;
        if (TryGetComponent<NodeController>(out _)) return GlitchContainerKind.Node;

        return null;
    }

    /// <summary>
    /// Load-time guard for data the Editor checks could not see (a prefab never opened in a scene):
    /// an invalid state is reported and replaced by Clean without raising the event, because no
    /// subscriber has read the initial state yet. The only write outside GlitchTransferManager.
    /// </summary>
    public void SanitizeOnLoad(GlitchContainerKind kind)
    {
        if (GlitchSwapRules.IsValidState(kind, _currentLevel)) return;

        Debug.LogError($"[GlitchComponent] {name}: a {kind} cannot start in {_currentLevel}. Starting Clean; " +
                       "fix the prefab or scene override.", this);

        _currentLevel = GlitchState.Clean;
    }

    /// <summary>The only state mutator in the project; only GlitchTransferManager calls it.</summary>
    public void SetState(GlitchState state)
    {
        if (state == _currentLevel) return;

        _currentLevel = state;
        OnGlitchStateChanged?.Invoke(_currentLevel);
    }
}
