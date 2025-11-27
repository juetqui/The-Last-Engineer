using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("View References")]
    [SerializeField] private GameObject _laserRendererPrefab;
    [SerializeField] private ParticleSystem _beamLaser;
    [SerializeField] private ParticleSystem _hitLaser;

    [Header("Distance Parameters")]
    [SerializeField] private int _maxDist = 100;
    [SerializeField] private float _raycastOffsetX = 2f;
    [SerializeField] private float _raycastOffsetZ = 1f;

    [Header("Initialization Parameter")]
    [SerializeField] private bool _startsInitialized = false;

    [Header("Turn On/Off Parameters")]
    [SerializeField] private LeanTweenType _easeType = LeanTweenType.easeInSine;
    [SerializeField] private float _easeTime = 0.5f;

    [Header("Raycast Filter Parameter")]
    [SerializeField] private LayerMask _laserLayer;

    [Header("Debug Parameter")]
    [SerializeField] private bool _debug = false;
    [SerializeField] private GameObject _debugObject;

    private LTDescr _currentTween = null;

    private LineRenderer _lineRenderer = null;
    private Glitcheable _glitcheable = null;
    private AudioSource _audioSource = default;
    private ILaserReceptor _ownReceptor = null;
    private ILaserReceptor _lastHit = null;
    private RaycastHit _rayHit;
    private Ray _ray, _leftRay, _rightRay;
    private float _currentDist = 0f, _lastTargetDist = 0f;
    private bool _isInitialized, _wasHit = false;

    private void Awake()
    {
        GameObject instancedLineRenderer = Instantiate(_laserRendererPrefab, null);

        _lineRenderer = instancedLineRenderer.GetComponent<LineRenderer>();
        _glitcheable = GetComponentInParent<Glitcheable>();
        _ownReceptor = GetComponentInParent<ILaserReceptor>();
        
        _audioSource = GetComponent<AudioSource>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.transform.position = Vector3.zero;

        Vector3 laserPos = GetFixedLaserPos();
        _lineRenderer.SetPosition(0, laserPos);
        _lineRenderer.SetPosition(1, laserPos);
        _lineRenderer.endWidth = _raycastOffsetX*2;
        _lineRenderer.startWidth = _raycastOffsetX*2;
    }

    private void Start()
    {
        if (_startsInitialized)
        {
            LaserRecived();
            _beamLaser.Play();
            CastLaser();
            _audioSource.Play();
        }
    }

    private void Update()
    {
        if (!_isInitialized)
        {
            if (_lastHit != null)
            {
                _lastHit.LaserNotRecived();
                _lastHit = null;
            }
            _audioSource.Stop();
            return;
        }

        if (!_audioSource.isPlaying)
            _audioSource.Play();

        if (_glitcheable == null)
        {
            SetLaserLength(_maxDist);
            CastLaser();
            CorruptionCheck();
        }
        else if (_glitcheable._sm.Current is IGlitchInterruptible)
        {
            if (_glitcheable._sm.Current is GlitchReintegratingState)
            {
                SetLaserLength(_maxDist);
            }
            else if (_glitcheable._sm.Current is GlitchDisintegratingState)
            {
                SetLaserLength(0);
            }

            _lineRenderer.enabled = true;
            CastLaser();
            CorruptionCheck();
        }
        else
        {
            SetLaserLength(0);
            _lineRenderer.enabled = false;
        }
    }

    private void CastLaser()
    {
        Vector3 laserOrigin = GetFixedLaserPos();
        Ray mainRay = new Ray(laserOrigin, transform.forward);

        RaycastHit hit;
        bool hasHit = Physics.Raycast(mainRay, out hit, _currentDist, _laserLayer);

        if (!hasHit)
        {
            ClearLastHit();
            SetLaserEnd(laserOrigin, laserOrigin + transform.forward * _currentDist);
            StopLaserEffect();
            return;
        }

        ProcessHit(laserOrigin, hit);
    }

    private void ProcessHit(Vector3 origin, RaycastHit hit)
    {
        SetLaserEnd(origin, hit.point);
        PlayLaserEffect(hit.point, hit.normal);

        if (hit.collider.TryGetComponent(out ILaserReceptor receptor) && receptor != _ownReceptor)
        {
            if (_lastHit != receptor)
            {
                if (_lastHit != null)
                    _lastHit.LaserNotRecived();

                _lastHit = receptor;
            }

            receptor.LaserRecived();
        }
        else
        {
            ClearLastHit();
        }
    }

    private void SetLaserEnd(Vector3 start, Vector3 end)
    {
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }

    private void PlayLaserEffect(Vector3 pos, Vector3 normal)
    {
        _hitLaser.transform.position = pos;
        _hitLaser.transform.rotation = Quaternion.LookRotation(normal);
        
        if (!_hitLaser.isPlaying) _hitLaser.Play();
    }

    private void StopLaserEffect()
    {
        if (_hitLaser.isPlaying) _hitLaser.Stop();
    }

    private void NotifyReceptor(ILaserReceptor receptor)
    {
        if (_lastHit != null && _lastHit != receptor)
            _lastHit.LaserNotRecived();

        _lastHit = receptor;
    }

    private void ClearLastHit()
    {
        if (_lastHit == null) return;

        _lastHit.LaserNotRecived();
        _lastHit = null;
    }

    private Vector3 GetFixedLaserPos()
    {
        return transform.position + transform.forward * _raycastOffsetZ;
    }

    public void LaserRecived()
    {
        if (_startsInitialized && _wasHit) return;

        _isInitialized = true;

        if (_wasHit) return;

        _wasHit = true;
        SetLaserLength(_maxDist);
    }

    public void LaserNotRecived()
    {
        SetLaserLength(0);

        if (!_startsInitialized)
        {
            _isInitialized = false;
            _wasHit = false;
        }

        StopLaserEffect();
    }

    protected virtual void CorruptionCheck()
    {

    }

    protected virtual bool CollitionCheck(RaycastHit hit)
    {
        return false;
    }

    private void SetLaserLength(int targetLength)
    {
        float correctedTarget = GetValidLaserDistance(targetLength);

        _lastTargetDist = correctedTarget;

        LeanTween.value(gameObject, _currentDist, correctedTarget, _easeTime)
            .setEase(_easeType)
            .setOnUpdate(v => UpdateCurrentDist(v))
            .setOnComplete(c => _currentDist = correctedTarget);
    }

    private float GetValidLaserDistance(float maxDistance)
    {
        Vector3 origin = GetFixedLaserPos();
        Ray ray = new Ray(origin, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _laserLayer))
        {
            return hit.distance;
        }

        return maxDistance;
    }

    private void UpdateCurrentDist(float value)
    {
        _currentDist = value;
        
        Vector3 origin = GetFixedLaserPos();
        _lineRenderer.SetPosition(0, origin);
        _lineRenderer.SetPosition(1, origin + transform.forward * _currentDist);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 rayPos = GetFixedLaserPos() + transform.right * _raycastOffsetX;
        Gizmos.DrawRay(rayPos, transform.forward * _maxDist);
        
        rayPos = GetFixedLaserPos() - transform.right * _raycastOffsetX;
        Gizmos.DrawRay(rayPos, transform.forward * _maxDist);
    }
}
