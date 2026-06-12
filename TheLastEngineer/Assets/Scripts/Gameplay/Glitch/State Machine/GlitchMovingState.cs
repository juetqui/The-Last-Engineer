using DG.Tweening;
using UnityEngine;

public class GlitchMovingState : IState
{
    private readonly Glitcheable g;
    private TimerController t;
    
    private IState _next;
    
    private float _elapsed;
    private float _duration;

    // Stored in parent-local space when _useLocalSpace is true,
    // otherwise stored as world-space values (no parent case).
    private Vector3 _startPos;    private Quaternion _startRot;
    private Vector3 _targetPos;   private Quaternion _targetRot;
    private bool _useLocalSpace;

    public GlitchMovingState(Glitcheable g) { this.g = g; }
    public void SetNext(IState next) { _next = next; }

    public void Enter()
    {
        t = g._timer;
        _elapsed = 0f;
        _duration = t ? t.MoveDuration : 0.5f;

        Transform parent = g.transform.parent;
        if (parent != null)
        {
            _useLocalSpace = true;
            _startPos  = parent.InverseTransformPoint(g.transform.position);
            _targetPos = parent.InverseTransformPoint(g.CurrentTargetPos);
            _startRot  = Quaternion.Inverse(parent.rotation) * g.transform.rotation;
            _targetRot = Quaternion.Inverse(parent.rotation) * g.CurrentTargetRot;
        }
        else
        {
            _useLocalSpace = false;
            _startPos  = g.transform.position;
            _targetPos = g.CurrentTargetPos;
            _startRot  = g.transform.rotation;
            _targetRot = g.CurrentTargetRot;
        }

        g.HologramSwitch();
        g.SetBoolCorrupted(0f);
        g.SetParticles(false, 1f);
        g.SetColliders(false);
    }

    public void Tick(float dt)
    {
        _elapsed += dt;

        float raw = Mathf.Clamp01(_elapsed / _duration);
        float e   = DOVirtual.EasedValue(0f, 1f, raw, Ease.InOutQuad);

        ApplyPose(Vector3.Lerp(_startPos, _targetPos, e),
                  Quaternion.Lerp(_startRot, _targetRot, e));

        if (raw >= 1f)
        {
            ApplyPose(_targetPos, _targetRot);
            g.AdvanceToNextNode();
            g.FSM?.Change(_next);
        }
    }

    public void Exit() { }

    // Converts local-or-world pose back to world space and applies it.
    private void ApplyPose(Vector3 pos, Quaternion rot)
    {
        if (_useLocalSpace)
        {
            Transform parent = g.transform.parent;
            if (parent != null)
            {
                g.transform.position = parent.TransformPoint(pos);
                g.transform.rotation = parent.rotation * rot;
                return;
            }
        }
        g.transform.position = pos;
        g.transform.rotation = rot;
    }
}
