using DG.Tweening;
using UnityEngine;

public class GlitchDisintegratingState : IState, IGlitchInterruptible
{
    private Glitcheable g;
    private TimerController t;
    
    private IState _nextNormal;
    private IState _nextInterrupt;

    private float _elapsed;
    private float _duration;
    private bool _collidersOff;

    public GlitchDisintegratingState(Glitcheable g) { this.g = g; }

    public void Enter()
    {
        t = g._timer;
        _elapsed = 0f; _collidersOff = false;
        _duration = t ? t.TransparencyDuration : 1f;

        g.SetDecal(1f);
        g.SetParticles(true, g._radialDonutPS);
        g.PlaySfx(g._sounds ? g._sounds.startSFX : null);
    }

    public void Tick(float dt)
    {
        _elapsed += dt;
        
        float raw = Mathf.Clamp01(_elapsed / _duration);
        float e = DOVirtual.EasedValue(0f, 1f, raw, Ease.InOutQuad);
        float alpha = Mathf.Lerp(1f, 0f, e);

        g.SetAlpha(alpha);
        g.SetFeedbackAlpha(1f - alpha);

        if (!_collidersOff && e >= 0.5f)
        {
            g.SetColliders(false);
            _collidersOff = true;
        }


        if (raw >= 1f)
        {
            g.SetParticles(false, 1f);
            g._sm?.Change(_nextNormal);
        }
    }

    public void Exit() { }

    public GlitchDisintegratingState ResetAndReturn()
    {
        _elapsed = 0f;
        _collidersOff = false;
        return this;
    }

    public void SetNext(IState nextNormal, IState nextInterrupt)
    {
        _nextNormal = nextNormal;
        _nextInterrupt = nextInterrupt;
    }

    public void Interrupt()
    {
        g._sm?.Change(_nextInterrupt);
    }
}
