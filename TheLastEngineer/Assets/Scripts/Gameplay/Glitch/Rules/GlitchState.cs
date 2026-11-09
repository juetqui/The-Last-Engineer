/// <summary>
/// Glitch state held by a container (node, Glitcheable or Intangible). Set/Take swap the whole state
/// between the node in hand and the object; which states each container accepts lives in
/// GlitchSwapRules.
///
/// Values are explicit because GlitchComponent._currentLevel serializes them as ints in prefabs and
/// scenes. There is no "no node" sentinel: ask PlayerNodeHandler.HasNode.
/// </summary>
public enum GlitchState
{
    Clean = 0,
    Intangible = 1,
    Glitched = 2
}
