/// <summary>
/// Already Clean, but the player is still inside: the layer and the visual stay intangible until the player
/// leaves, otherwise they would be stuck or pushed out. Clean applies both when they do.
/// </summary>
public class IntangibleExitingState : IState
{
    private readonly Intangible _owner;

    public IntangibleExitingState(Intangible owner) { _owner = owner; }

    public void Enter() { }

    // Polled: nothing raises an event when the player walks out.
    public void Tick(float dt)
    {
        if (!_owner.IsPlayerInside()) _owner.EnterClean();
    }

    public void Exit() { }
}
