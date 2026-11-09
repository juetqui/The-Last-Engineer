/// <summary>An object the player can Set a state into or Take it from (Glitcheable or Intangible).</summary>
public interface IGlitchSwapTarget : IInteractable
{
    GlitchComponent Glitch { get; }
    GlitchContainerKind Kind { get; }

    /// <summary>A Set/Take aimed at this object was rejected: play its own rejection feedback.</summary>
    void NotifyRejected();
}
