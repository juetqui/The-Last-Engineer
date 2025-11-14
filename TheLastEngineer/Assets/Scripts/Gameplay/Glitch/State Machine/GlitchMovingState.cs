using DG.Tweening;
using UnityEngine;

public class GlitchMovingState : IState
{
    private readonly Glitcheable g;
    private TimerController t;
    
    private IState _next;
    
    private float _elapsed;
    private float _duration;
    
    private Vector3 _startPos; private Quaternion _startRot;
    private Vector3 _targetPos; private Quaternion _targetRot;


    public GlitchMovingState(Glitcheable g) { this.g = g; }
    public void SetNext(IState next) { _next = next; }

    public void Enter()
    {
        t = g._timer;
        _elapsed = 0f; 
        _duration = t ? t.MoveDuration : 0.5f;
        _startPos = g.transform.position; _startRot = g.transform.rotation;
        _targetPos = g.CurrentTargetPos; _targetRot = g.CurrentTargetRot;
        g.HologramSwitch();
        g.SetBoolCorrupted(0f);
        g.SetParticles(false, 1f);
        g.SetColliders(false);
    }


    public void Tick(float dt)
    {
        _elapsed += dt;
        
        float raw = Mathf.Clamp01(_elapsed / _duration);
        float e = DOVirtual.EasedValue(0f, 1f, raw, Ease.InOutQuad);

        g.transform.position = Vector3.Lerp(_startPos, _targetPos, e);
        g.transform.rotation = Quaternion.Lerp(_startRot, _targetRot, e);

        if (raw >= 1f)
        {
            g.transform.position = _targetPos;
            g.transform.rotation = _targetRot;
            g.AdvanceToNextNode();
            g._sm?.Change(_next);
        }
    }

    public void Exit() { }
}
