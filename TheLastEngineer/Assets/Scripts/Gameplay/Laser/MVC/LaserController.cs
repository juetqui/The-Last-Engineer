using System;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int _maxDist = 100;
    [SerializeField] private float _offsetZ = 1f;
    [SerializeField] private float _easeTime = 0.5f;
    [SerializeField] private LayerMask _layer;
    [SerializeField] private bool _startsInitialized = true;
    [SerializeField] private bool _debug = false;
    [SerializeField] private Color _gizmosColor = Color.red;

    private LaserModel _model;
    private LaserView _view;

    private bool _isInitialized = false;
    private bool _isIdle = false;
    private bool _isTransitioning = false;
    private bool _isToggling = false;
    private bool _idleSetupDone = false;

    private ILaserReceptor _ownReceptor;
    private Glitcheable _glitcheable;

    private void Awake()
    {
        _model = new LaserModel(_maxDist, _layer, _easeTime, _debug);
        _view = GetComponent<LaserView>();

        _view.Init(1);
        _ownReceptor = GetComponentInParent<ILaserReceptor>();
        _glitcheable = GetComponentInParent<Glitcheable>();
        
        if (_glitcheable != null)
            _glitcheable.FSM.OnStateChanged += UpdateGlitchedBehaviour;
        
        _isInitialized = _startsInitialized;
    }

    private void OnDestroy()
    {
        if (_glitcheable != null)
            _glitcheable.FSM.OnStateChanged -= UpdateGlitchedBehaviour;
    }

    private void Update()
    {
        if (_glitcheable != null)
            CheckGlitchedBehaviour();
        else
            CheckCommonBehaviour();
    }

    private void CheckGlitchedBehaviour()
    {
        if (_isIdle) IdleBehaviour();
        else if (_isTransitioning) TransitioningBehaviour();
    }

    private void UpdateGlitchedBehaviour(IState newState)
    {
        (_isIdle, _isTransitioning) = newState switch
        {
            GlitchReintegratingState or GlitchIdleState => (true, false),
            GlitchDisintegratingState or GlitchMovingState => (false, true),
            _ => (false, false)
        };

        _isToggling = false;
        _idleSetupDone = false;
    }

    private void IdleBehaviour()
    {
        if (!_isInitialized)
        {
            if (!_isToggling) return;

            _model.UpdateRaycastDistance();
            var origin = GetLaserOrigin();
            var dir = transform.forward;
            _view.SetLaserPositions(origin, origin + dir * _model.CurrentDist);

            if (_model.IsTransitioning) return;

            _isToggling = false;
            _view.EnableBeam(false);
            _view.StopHitEffect();
            _view.StopAudio();
            return;
        }

        _view.EnableBeam(true);

        if (!_idleSetupDone)
        {
            var hit = CheckRaycast(out var origin, out var dir, out var didHit);
            
            _model.SetLaserLength(didHit ? hit.distance : _maxDist);
            _isToggling = true;
            _idleSetupDone = true;
        }

        CheckCommonBehaviour();
    }

    private void TransitioningBehaviour()
    {
        if (!_isInitialized) return;
        
        if (!_isToggling)
        {
            _isToggling = true;
            _model.SetLaserLength(0f);
        }

        _model.ClearReceptor();
        _model.UpdateRaycastDistance();

        var origin = GetLaserOrigin();
        var dir = transform.forward;
        var end = origin + dir * _model.CurrentDist;

        _view.SetLaserPositions(origin, end);

        if (_model.IsTransitioning) return;

        _isToggling = false;
        _view.EnableBeam(false);
        _view.StopHitEffect();
        _view.StopAudio();
    }

    private RaycastHit CheckRaycast(out Vector3 origin, out Vector3 dir, out bool didHit)
    {
        origin = GetLaserOrigin();
        dir = transform.forward;

        didHit = Physics.Raycast(origin, dir, out var hit, _maxDist, _layer, QueryTriggerInteraction.Ignore);
        return hit;
    }

    private void CheckCommonBehaviour()
    {
        var hit = CheckRaycast(out var origin, out var dir, out var didHit);

        if (_isToggling && _model.IsTransitioning)
        {
            _model.ClearReceptor();
            _model.UpdateRaycastDistance();
            _view.SetLaserPositions(origin, origin + dir * _model.CurrentDist);
            _view.StopHitEffect();
            return;
        }

        _isToggling = false;
        _model.SetInstant(_isInitialized ? (didHit ? hit.distance : _maxDist) : 0f);

        if (!_isInitialized) return;
        
        if (didHit) _model.ProcessReceptor(hit, _ownReceptor);
        else _model.ClearReceptor();

        _model.UpdateRaycastDistance();

        var end = origin + dir * _model.CurrentDist;
        
        _view.SetLaserPositions(origin, end);

        if (didHit)
            _view.ShowHitEffect(hit.point, hit.normal);
        else
            _view.StopHitEffect();
    }

    public void LaserReceived()
    {
        if (_startsInitialized) return;

        _isInitialized = true;
        _isToggling = true;

        var hit = CheckRaycast(out var origin, out var dir, out var didHit);

        _model.SetLaserLength(didHit ? hit.distance : _maxDist);
    }

    public void LaserNotReceived()
    {
        if (_startsInitialized) return;

        _isInitialized = false;
        _isToggling = true;
        _idleSetupDone = false;
        _model.ClearReceptor();
        _model.SetLaserLength(0f);
    }

    private Vector3 GetLaserOrigin()
    {
        return transform.position + transform.forward * _offsetZ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        
        var maxDist = _model?.CurrentDist ?? _maxDist;

        var origin = transform.position + transform.forward * _offsetZ;
        var dir = transform.forward;
        var end = origin + dir * maxDist;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(origin, 0.05f);
    }
}
