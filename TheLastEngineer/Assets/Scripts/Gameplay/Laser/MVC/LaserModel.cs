using UnityEngine;

public class LaserModel
{
    public int MaxDistance { get; private set; }
    public LayerMask LaserLayer { get; private set; }
    public float CurrentDist { get; private set; }
    public float TargetDist { get; private set; }

    private float _startDist = 0f;
    private float _transitionTimer = 0f;
    private float _easeTime = 0f;

    public bool IsTransitioning => _isTransitioning;

    private bool _isTransitioning = false;
    private bool _debug = false;

    private ILaserReceptor _lastHit;
    private GlitchState _lastState;

    public LaserModel(int maxDist, LayerMask layer, float easeTime, bool debug)
    {
        MaxDistance = maxDist;
        LaserLayer = layer;
        _easeTime = easeTime;
        _debug = debug;
    }

    // The state sent on enter is stored and sent again on exit, so the receptor's per-state count
    // always pairs up. A change of the emitter's state while hitting the same receptor re-notifies.
    public void ProcessReceptor(RaycastHit hit, ILaserReceptor own, GlitchState emitterState)
    {
        if (!hit.collider.TryGetComponent(out ILaserReceptor receptor) || receptor == own)
        {
            ClearReceptor();
            return;
        }

        if (receptor == _lastHit && emitterState == _lastState) return;

        _lastHit?.LaserNotReceived(_lastState);
        _lastHit = receptor;
        _lastState = emitterState;
        _lastHit.LaserReceived(_lastState);
    }

    public void ClearReceptor()
    {
        _lastHit?.LaserNotReceived(_lastState);
        _lastHit = null;
    }

    public void SetLaserLength(float length)
    {
        _startDist = CurrentDist;
        TargetDist = length;
        _transitionTimer = 0f;
        _isTransitioning = true;
    }

    public void SetInstant(float length)
    {
        CurrentDist = length;
        TargetDist = length;
        _transitionTimer = 0f;
        _isTransitioning = false;
    }

    public float UpdateRaycastDistance()
    {
        if (!_isTransitioning)
            return CurrentDist;

        _transitionTimer += Time.deltaTime;
        var t = _transitionTimer / _easeTime;

        CurrentDist = Mathf.Lerp(_startDist, TargetDist, t);

        if (t >= 1f)
            _isTransitioning = false;

        return CurrentDist;
    }
}
