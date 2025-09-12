using System.Linq;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private ParticleSystem _beamLaser;
    [SerializeField] private ParticleSystem _hitLaser;
    [SerializeField] private float _maxDist = 20f;
    [SerializeField] private float _raycastOffsetX = 2f;
    [SerializeField] private float _raycastOffsetZ = 1f;
    [SerializeField] private bool _startsInitialized = false;
    private bool _isInitialized;
    [SerializeField] private LayerMask _laserLayer;

    private ILaserReceptor _lastHit = null;
    private RaycastHit _rayHit;
    private Ray _ray, _leftRay, _rightRay;
    private Glitcheable _glitcheable = null;

    private AudioSource _audioSource = default;

    private void Awake()
    {
        _glitcheable = GetComponentInParent<Glitcheable>();
        
        _audioSource = GetComponent<AudioSource>();
        _lineRenderer.positionCount = 2;

        Vector3 laserPos = GetFixedLaserPos();

        _lineRenderer.SetPosition(0, laserPos);
        _lineRenderer.SetPosition(1, laserPos);
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
        RaycastHit[] hits = Physics.RaycastAll(mainRay, _maxDist, _laserLayer);
        
        if (hits.Length > 0)
        {
            var closestHit = hits.OrderBy(h => h.distance).First();
            ProcessHit(laserOrigin, closestHit);
        }
        else
        {
            SetLaserEnd(laserOrigin, laserOrigin + transform.forward * _maxDist);
            StopLaserEffect();
        }
    }

    private void ProcessHit(Vector3 origin, RaycastHit hit)
    {
        SetLaserEnd(origin, hit.point);

        PlayLaserEffect(hit.point, hit.normal);

        if (hit.collider.TryGetComponent(out ILaserReceptor receptor))
        {
            NotifyReceptor(receptor);
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
        return new Vector3(transform.position.x, transform.position.y, transform.position.z) + transform.forward * _raycastOffsetZ;
    }

    public void LaserRecived()
    {
        _isInitialized = true;
    }

    public void LaserNotRecived()
    {
        Vector3 laserPos = GetFixedLaserPos();
        
        if(!_startsInitialized) _isInitialized = false;

        _lineRenderer.SetPosition(0, laserPos);
        _lineRenderer.SetPosition(1, laserPos);
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
