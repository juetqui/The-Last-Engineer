/// <summary>Passable for a player carrying an Intangible node: intangible layer, overlay on.</summary>
public class IntangiblePhasedState : IState
{
    private readonly Intangible _owner;

    public IntangiblePhasedState(Intangible owner) { _owner = owner; }

    public void Enter()
    {
        _owner.ApplyLayer(true);
        _owner.FadeVisual(true);
    }

    public void Tick(float dt) { }
    public void Exit() { }
}
