/// <summary>
/// Already Clean, but the player is still inside: the visual fades out now and the layer stays
/// intangible until the player leaves, otherwise they would be stuck or pushed out.
/// </summary>
public class IntangibleExitingState : IState
{
    private readonly Intangible _owner;

    public IntangibleExitingState(Intangible owner) { _owner = owner; }

    public void Enter() => _owner.FadeVisual(false);

    // Polled: nothing raises an event when the player walks out.
    public void Tick(float dt)
    {
        if (!_owner.IsPlayerInside()) _owner.EnterClean();
    }

    public void Exit() { }
}
