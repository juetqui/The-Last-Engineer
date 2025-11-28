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
    
    private bool _isTransitioning = false;
    private bool _debug = false;

    private ILaserReceptor _lastHit;

    public LaserModel(int maxDist, LayerMask layer, float easeTime, bool debug)
    {
        MaxDistance = maxDist;
        LaserLayer = layer;
        _easeTime = easeTime;
        _debug = debug;
    }

    public void ProcessReceptor(RaycastHit hit, ILaserReceptor own)
    {
        if (!hit.collider.TryGetComponent(out ILaserReceptor receptor) || receptor == own)
        {
            ClearReceptor();
            return;
        }

        if (_lastHit != receptor)
        {
            _lastHit?.LaserNotRecived();
            _lastHit = receptor;
        }

        _lastHit.LaserRecived();
    }

    public void ClearReceptor()
    {
        _lastHit?.LaserNotRecived();
        _lastHit = null;
    }

    public void SetLaserLength(float length)
    {
        _startDist = CurrentDist;
        TargetDist = length;
        _transitionTimer = 0f;
        _isTransitioning = true;
    }

    public float UpdateRaycastDistance()
    {
        if (!_isTransitioning)
            return CurrentDist;

        _transitionTimer += Time.deltaTime;
        float t = _transitionTimer / _easeTime;

        CurrentDist = Mathf.Lerp(_startDist, TargetDist, t);

        if (t >= 1f)
            _isTransitioning = false;

        return CurrentDist;
    }
}
