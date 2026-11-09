/// <summary>Solid: default layer, no overlay.</summary>
public class IntangibleCleanState : IState
{
    private readonly Intangible _owner;

    public IntangibleCleanState(Intangible owner) { _owner = owner; }

    public void Enter()
    {
        _owner.ApplyLayer(false);
        _owner.FadeVisual(false);
    }

    public void Tick(float dt) { }
    public void Exit() { }
}
