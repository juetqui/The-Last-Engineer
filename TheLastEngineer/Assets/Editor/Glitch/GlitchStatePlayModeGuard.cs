using UnityEditor;
using UnityEngine;

/// <summary>
/// Refuses to enter Play Mode while an open scene holds invalid glitch data: a GlitchComponent in
/// a state its owner doesn't accept (Intangible on a Glitcheable, Glitched on an Intangible), or a
/// GameObject with both Glitcheable and Intangible. Each offender gets a LogError with context.
/// Only open scenes are checked; at runtime GlitchComponent.SanitizeOnLoad covers the rest.
/// </summary>
[InitializeOnLoad]
public static class GlitchStatePlayModeGuard
{
    static GlitchStatePlayModeGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.ExitingEditMode) return;
        if (!HasInvalidGlitchData()) return;

        Debug.LogError("[GlitchStatePlayModeGuard] Play Mode cancelled: fix the glitch errors above.");
        EditorApplication.isPlaying = false;
    }

    private static bool HasInvalidGlitchData()
    {
        bool invalid = false;

        foreach (var glitch in Object.FindObjectsByType<GlitchComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            bool isGlitcheable = glitch.TryGetComponent<Glitcheable>(out _);
            bool isIntangible = glitch.TryGetComponent<Intangible>(out _);

            if (isGlitcheable && isIntangible)
            {
                Debug.LogError($"[GlitchStatePlayModeGuard] {Path(glitch)} has both Glitcheable and Intangible: keep only one.", glitch);
                invalid = true;
                continue;
            }

            var kind = glitch.ResolveKind();
            if (kind == null || GlitchSwapRules.IsValidState(kind.Value, glitch.CurrentState)) continue;

            Debug.LogError($"[GlitchStatePlayModeGuard] {Path(glitch)}: a {kind.Value} cannot start in {glitch.CurrentState}.", glitch);
            invalid = true;
        }

        return invalid;
    }

    private static string Path(Component c) => $"{c.gameObject.scene.name}/{c.transform.root.name}/{c.name}";
}
