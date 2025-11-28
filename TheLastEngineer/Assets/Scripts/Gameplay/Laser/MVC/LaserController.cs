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

    private LaserModel _model = default;
    private LaserView _view = default;

    private bool _isInitialized = false;
    private ILaserReceptor _ownReceptor = default;
    private Glitcheable _glitcheable = default;

    private void Awake()
    {
        _model = new LaserModel(_maxDist, _layer, _easeTime, _debug);
        _view = GetComponent<LaserView>();

        _view.Init(1);
        _ownReceptor = GetComponentInParent<ILaserReceptor>();
        _glitcheable = GetComponentInParent<Glitcheable>();
    }

    private void Start()
    {
        _isInitialized = _startsInitialized;
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
        if (_glitcheable.FSM.Current is GlitchReintegratingState || _glitcheable.FSM.Current is GlitchIdleState)
        {
            _view.EnableBeam(true);
            CheckCommonBehaviour();
        }
        else if (_glitcheable.FSM.Current is GlitchDisintegratingState || _glitcheable.FSM.Current is GlitchMovingState)
        {
            _model.SetLaserLength(0f);
            _model.ClearReceptor();
            _model.UpdateRaycastDistance();

            Vector3 origin = GetLaserOrigin();
            Vector3 dir = transform.forward;
            Vector3 end = origin + dir * _model.CurrentDist;

            _view.SetLaserPositions(origin, end);

            if (_model.CurrentDist <= 0.01f)
            {
                _view.EnableBeam(false);
                _view.StopHitEffect();
                _view.StopAudio();
            }
        }
    }

    private void CheckCommonBehaviour()
    {
        Vector3 origin = GetLaserOrigin();
        Vector3 dir = transform.forward;

        RaycastHit hit;

        bool didHit = Physics.Raycast(origin, dir, out hit, _model.TargetDist, _layer, QueryTriggerInteraction.Ignore);

        if (_debug)
        {
            Debug.Log(didHit ? $"Hit: {hit.collider.name}" : "No Hit");
        }

        if (_isInitialized)
            _model.SetLaserLength(didHit ? hit.distance : _maxDist);
        else
            _model.SetLaserLength(0f);

        if (didHit)
            _model.ProcessReceptor(hit, _ownReceptor);
        else
            _model.ClearReceptor();

        _model.UpdateRaycastDistance();

        Vector3 start = origin;
        Vector3 end = origin + dir * _model.CurrentDist;
        
        _view.SetLaserPositions(start, end);

        if (didHit)
            _view.ShowHitEffect(hit.point, hit.normal);
        else
            _view.StopHitEffect();
    }

    public void LaserRecived()
    {
        if (_startsInitialized) return;

        _isInitialized = true;
    }

    public void LaserNotRecived()
    {
        if (_startsInitialized) return;

        _isInitialized = false;
    }

    private Vector3 GetLaserOrigin()
    {
        return transform.position + transform.forward * _offsetZ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;
        
        float maxDist = _model != null ? _model.CurrentDist : _maxDist;   

        Vector3 origin = transform.position + transform.forward * _offsetZ;
        Vector3 dir = transform.forward;
        Vector3 end = origin + dir * maxDist;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(origin, 0.05f);
    }
}
