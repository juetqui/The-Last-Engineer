using UnityEngine;

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

        if (g.handrails != null)
        {
            foreach (var h in g.handrails)
            {
                h.SetActive(false);
            }
        }

        if (!g.IsPlatform) return;

        g.transform.SetParent(g.CurrentTarget);
        g.transform.localPosition = Vector3.zero;
    }

    public void Tick(float dt) { return; }
    public void Exit()
    {
        if (g.handrails == null) return;

        foreach (var h in g.handrails)
        {
            h.SetActive(false);
        }
    }
    public void Interrupt()
    {
        g.FSM.Change(_next);
    }
}
