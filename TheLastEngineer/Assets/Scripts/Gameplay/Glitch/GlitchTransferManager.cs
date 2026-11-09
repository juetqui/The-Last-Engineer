/// <summary>
/// The only place where glitch states change hands. Set and Take swap the whole state between the
/// node in hand and the object; GlitchSwapRules decides what is allowed.
///
/// Returns a result instead of firing feedback: rumble and sounds are GlitchTransferHandler's job,
/// on the player side, so input and audio stay out of here.
/// </summary>
public static class GlitchTransferManager
{
    /// <summary>Same validation as Set but with no effects, for the HUD and target selection.</summary>
    public static GlitchTransferResult PreviewSet(GlitchComponent node, IGlitchSwapTarget target)
    {
        // UnityEngine.Object == null also covers destroyed objects.
        if (node == null) return GlitchTransferResult.NoSource;
        if (!IsAlive(target)) return GlitchTransferResult.NoTarget;

        return GlitchSwapRules.EvaluateSet(node.CurrentState, target.Kind, target.Glitch.CurrentState);
    }

    /// <summary>Same validation as Take but with no effects, for the HUD and target selection.</summary>
    public static GlitchTransferResult PreviewTake(GlitchComponent node, IGlitchSwapTarget target)
    {
        if (node == null) return GlitchTransferResult.NoSource;
        if (!IsAlive(target)) return GlitchTransferResult.NoTarget;

        return GlitchSwapRules.EvaluateTake(node.CurrentState, target.Glitch.CurrentState);
    }

    public static GlitchTransferResult Set(GlitchComponent node, IGlitchSwapTarget target)
    {
        var result = PreviewSet(node, target);
        if (result == GlitchTransferResult.Swapped) Swap(node, target.Glitch);

        return result;
    }

    public static GlitchTransferResult Take(GlitchComponent node, IGlitchSwapTarget target)
    {
        var result = PreviewTake(node, target);
        if (result == GlitchTransferResult.Swapped) Swap(node, target.Glitch);

        return result;
    }

    // Both sides were validated before: one of them is always Clean, so the swap cannot leave an
    // invalid state behind.
    private static void Swap(GlitchComponent node, GlitchComponent target)
    {
        var nodeState = node.CurrentState;

        node.SetState(target.CurrentState);
        target.SetState(nodeState);
    }

    private static bool IsAlive(IGlitchSwapTarget target)
    {
        return target is UnityEngine.Object obj && obj != null && target.Glitch != null;
    }
}
