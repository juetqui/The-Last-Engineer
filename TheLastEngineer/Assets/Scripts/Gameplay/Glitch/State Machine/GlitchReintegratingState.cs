using DG.Tweening;
using UnityEngine;

public class GlitchReintegratingState : IState, IGlitchInterruptible
{
    private readonly Glitcheable g;
    private TimerController t;
    
    private IState _next;
    private IState _nextInterrupt;
    
    private float _elapsed;
    private float _duration;
    private float _startAlpha;

    public GlitchReintegratingState(Glitcheable g) { this.g = g; }
    public void SetNext(IState next, IState nextInterrupt)
    {
        _next = next;
        _nextInterrupt = nextInterrupt;
    }

    private float ReadCurrentAlpha()
    {
        if (g._renderer == null || g._renderer.material == null) return 0f;
        return Mathf.Clamp01(g._renderer.material.GetFloat("_Alpha"));
    }

    public void Enter()
    {
        t = g._timer;
        _elapsed = 0f; _duration = t ? t.TransparencyDuration : 1f;
        _startAlpha = ReadCurrentAlpha();


        g.SetDecal(1f);
        g.SetParticles(true, 1f);
        g.PlaySfx(g._sounds ? g._sounds.endSFX : null);
        g.SetColliders(true);
    }

    public void Tick(float dt)
    {
        _elapsed += dt;
        float raw = Mathf.Clamp01(_elapsed / _duration);
        float e = DOVirtual.EasedValue(0f, 1f, raw, Ease.InOutQuad);

        float alpha = Mathf.Lerp(_startAlpha, 1f, e);
        g.SetAlpha(alpha);
        g.SetFeedbackAlpha(1f - alpha);


        if (raw >= 1f)
        {
            g.SetParticles(false, 1f);
            g.SetDecal(0f);
            g._sm?.Change(_next);
        }
    }

    public void Exit() { }

    public void Interrupt()
    {
        g._sm?.Change(_nextInterrupt);
    }
}
