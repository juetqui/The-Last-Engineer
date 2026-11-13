/// <summary>Values for [DefaultExecutionOrder] where the Update order between systems matters.</summary>
public static class ExecutionOrder
{
    /// <summary>
    /// Moving platforms move before Glitcheable (-1, which follows the platform anchors) and before the
    /// player (0), so the player applies the platform displacement of the same frame.
    /// </summary>
    public const int Platform = -50;
}
