public class GlitchIdleState : IState, IGlitchInterruptible
{
    private readonly Glitcheable g;
    
    private IState _next;
    
    public GlitchIdleState(Glitcheable g) { this.g = g; }

    public void SetNext(IState next)
    {
        _next = next;
    }
    
    public void Enter()
    {
        g.SetAlpha(1f);
        g.SetFeedbackAlpha(0f);
        g.SetDecal(0f);
        g.SetParticles(false, 1f);
        g.SetColliders(true);
    }

    public void Tick(float dt) { return; }
    public void Exit() { }
    public void Interrupt()
    {
        g._sm.Change(_next);
    }
}
