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
        g.SetBoolCorrupted(0f);
        g.SetParticles(false, 1f);
        g.SetColliders(true);
        if(g.Barandas!=null)
        for (int i = 0; i<g.Barandas.Length; i++)
        {
            g.Barandas[i].SetActive(false);
        }
        //desactivar pbarandas escaleras
    }

    public void Tick(float dt) { return; }
    public void Exit() {
        if (g.Barandas != null)

            for (int i = 0; i < g.Barandas.Length; i++)
        {
            g.Barandas[i].SetActive(true);
        }
    }
    public void Interrupt()
    {
        g.FSM.Change(_next);
    }
}
