#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Glitcheable and Intangible are mutually exclusive on a GameObject. Called from Reset (which the
/// Editor runs when a component is added): the component already there wins, the new one goes.
/// </summary>
public static class GlitchContainerExclusivity
{
    public static void RejectIfPresent<TOther>(MonoBehaviour added) where TOther : Component
    {
        if (!added.TryGetComponent<TOther>(out _)) return;

        Debug.LogWarning($"[{added.GetType().Name}] {added.name} already has a {typeof(TOther).Name}: " +
                         "Glitcheable and Intangible can't share a GameObject. Removing the one just added.", added);

        // Destroying inside Reset breaks the Inspector that is adding the component.
        EditorApplication.delayCall += () =>
        {
            if (added != null) Undo.DestroyObjectImmediate(added);
        };
    }
}
#endif
