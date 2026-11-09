/// <summary>
/// Pure rules of the Set/Take state swap. No Unity types so EditMode tests can reach them;
/// GlitchTransferManager applies what these allow.
/// </summary>
public static class GlitchSwapRules
{
    /// <summary>The only non-Clean state each object container can hold. A node holds any state.</summary>
    public static GlitchState ChargedStateOf(GlitchContainerKind kind)
    {
        return kind == GlitchContainerKind.Intangible ? GlitchState.Intangible : GlitchState.Glitched;
    }

    public static bool IsValidState(GlitchContainerKind kind, GlitchState state)
    {
        if (kind == GlitchContainerKind.Node) return true;

        return state == GlitchState.Clean || state == ChargedStateOf(kind);
    }

    /// <summary>Set: the node gives its whole state to a Clean object that accepts it.</summary>
    public static GlitchTransferResult EvaluateSet(GlitchState node, GlitchContainerKind targetKind, GlitchState target)
    {
        if (node == GlitchState.Clean) return GlitchTransferResult.NodeClean;
        if (target != GlitchState.Clean) return GlitchTransferResult.TargetNotClean;
        if (node != ChargedStateOf(targetKind)) return GlitchTransferResult.Incompatible;

        return GlitchTransferResult.Swapped;
    }

    /// <summary>Take: a Clean node pulls the whole state out of a charged object.</summary>
    public static GlitchTransferResult EvaluateTake(GlitchState node, GlitchState target)
    {
        if (node != GlitchState.Clean) return GlitchTransferResult.NodeNotClean;
        if (target == GlitchState.Clean) return GlitchTransferResult.TargetClean;

        return GlitchTransferResult.Swapped;
    }
}
