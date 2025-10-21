using UnityEngine;

public class MovingState : IPlatformState
{
    private PlatformController _pc;
    private bool _isDecelerating = false;

    public MovingState(PlatformController pc) { _pc = pc; }

    public void Enter()
    {
        _isDecelerating = false;
        _pc.StartAccelerationToMax();
    }

    public void Tick(float d)
    {
        if (_pc.ReachedTarget())
        {
            if (_pc.CheckStop())
            {
                _pc.StopImmediate();
                _pc.StopPassenger();
                _pc.AdvanceRouteAndWait();
                return;
            }

            _pc.Route.Advance();
        }

        if (!_isDecelerating && _pc.CheckStop())
        {
            Vector3 target = _pc.Route.CurrentPoint;
            float dist = Vector3.Distance(_pc.transform.position, target);

            float a = _pc.MoveSpeed / Mathf.Max(0.01f, _pc.DecelTime);
            float v = Mathf.Max(_pc.CurrentSpeed, 0.001f);
            float brakeDistance = (v * v) / (2f * a);

            if (dist <= brakeDistance)
            {
                _isDecelerating = true;
                _pc.DecelerateToTarget(target);
            }
        }

        _pc.MoveStep();
    }

    public void Exit() { }
}