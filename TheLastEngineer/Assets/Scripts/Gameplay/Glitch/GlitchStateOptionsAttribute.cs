using UnityEngine;

/// <summary>
/// Limits a GlitchState field's Inspector dropdown to the given states (GlitchStateOptionsDrawer).
/// Editor-only filtering: the owner still validates the value at runtime.
/// </summary>
public class GlitchStateOptionsAttribute : PropertyAttribute
{
    public GlitchState[] Options { get; }

    public GlitchStateOptionsAttribute(params GlitchState[] options)
    {
        Options = options;
    }
}
