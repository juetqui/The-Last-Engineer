using System.Linq;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private GameObject _laserRendererPrefab;
    [SerializeField] private ParticleSystem _beamLaser;
    [SerializeField] private ParticleSystem _hitLaser;
    [SerializeField] private float _maxDist = 20f;
    [SerializeField] private float _raycastOffsetX = 2f;
    [SerializeField] private float _raycastOffsetZ = 1f;
    [SerializeField] private bool _startsInitialized = false;
    [SerializeField] private LayerMask _laserLayer;

    [SerializeField] private bool _debug = false;

    private LineRenderer _lineRenderer = null;
    private Glitcheable _glitcheable = null;
    private AudioSource _audioSource = default;
    private ILaserReceptor _ownReceptor = null;
    private ILaserReceptor _lastHit = null;
    private RaycastHit _rayHit;
    private Ray _ray, _leftRay, _rightRay;
    private bool _isInitialized;

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
            _isInitialized = true;
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
            CastLaser();
            CorruptionCheck();
        }
        else if (_glitcheable._sm.Current is IGlitchInterruptible)
        {
            _lineRenderer.enabled = true;
            CastLaser();
            CorruptionCheck();
        }
        else _lineRenderer.enabled = false;
    }

    private void CastLaser()
    {
        Vector3 laserOrigin = GetFixedLaserPos();
        Ray mainRay = new Ray(laserOrigin, transform.forward);

        RaycastHit hit;
        bool hasHit = Physics.Raycast(mainRay, out hit, _maxDist, _laserLayer);

        if (!hasHit)
        {
            ClearLastHit();
            SetLaserEnd(laserOrigin, laserOrigin + transform.forward * _maxDist);
            StopLaserEffect();
            return;
        }

        ProcessHit(laserOrigin, hit);
    }

    private void ProcessHit(Vector3 origin, RaycastHit hit)
    {
        SetLaserEnd(origin, hit.point);
        PlayLaserEffect(hit.point, hit.normal);

        if (hit.collider.TryGetComponent(out ILaserReceptor receptor))
        {
            if (receptor != _ownReceptor)
            {
                if (_lastHit != receptor)
                {
                    if (_lastHit != null)
                        _lastHit.LaserNotRecived();

                    _lastHit = receptor;
                }

                receptor.LaserRecived();
            }
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
        _isInitialized = true;
    }

    public void LaserNotRecived()
    {
        Vector3 laserPos = GetFixedLaserPos();

        if (!_startsInitialized) _isInitialized = false;

        _lineRenderer.SetPosition(0, laserPos);
        _lineRenderer.SetPosition(1, laserPos);
        StopLaserEffect();
    }

    protected virtual void CorruptionCheck()
    {

    }

    protected virtual bool CollitionCheck(RaycastHit hit)
    {
        return false;
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
