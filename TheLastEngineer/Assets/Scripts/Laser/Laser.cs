using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _maxDist = 20f;
    [SerializeField] private float _raycastOffsetX = 2f;
    [SerializeField] private float _raycastOffsetZ = 1f;
    [SerializeField] private bool _startsInitialized = false;
    [SerializeField] private LayerMask _laserLayer;

    private ILaserReceptor _lastHit = null;
    private RaycastHit _rayHit;
    private Ray _ray, _leftRay, _rightRay;

    private AudioSource _audioSource = default;

    private void Awake()
    {
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
            CastLaser();
            _audioSource.Play();
        }
    }

    private void Update()
    {
        if (!_startsInitialized)
        {
            _audioSource.Stop();
            return;
        }

        if (!_audioSource.isPlaying)
            _audioSource.Play();

        CastLaser();
        CorruptionCheck();
    }

    private void CastLaser()
    {
        Vector3 laserPos = GetFixedLaserPos();

        _ray = new Ray(laserPos, transform.forward);
        _rightRay = new Ray(laserPos + transform.right * _raycastOffsetX, transform.forward);
        _leftRay = new Ray(laserPos - transform.right * _raycastOffsetX, transform.forward);

        if (Physics.Raycast(_rightRay, out _rayHit, _maxDist, _laserLayer))
        {
            if(_rayHit.collider.TryGetComponent(out PlayerTDController player))
            {
                if (_lastHit != null)
                    _lastHit.LaserNotRecived();

                _lineRenderer.SetPosition(0, laserPos);
                _lineRenderer.SetPosition(1, laserPos + (transform.forward * _maxDist));
                player.LaserRecived();
                _lastHit = null;
            }
        }

        if (Physics.Raycast(_leftRay, out _rayHit, _maxDist, _laserLayer))
        {
            if(_rayHit.collider.TryGetComponent(out PlayerTDController player))
            {
                if (_lastHit != null)
                    _lastHit.LaserNotRecived();

                _lineRenderer.SetPosition(0, laserPos);
                _lineRenderer.SetPosition(1, laserPos + (transform.forward * _maxDist));
                player.LaserRecived();
                _lastHit = null;
            }
        }

        if (Physics.Raycast(_ray, out _rayHit, _maxDist, _laserLayer))
        {
            if (_rayHit.collider.TryGetComponent(out PlayerTDController player))
            {
                if (_lastHit != null)
                    _lastHit.LaserNotRecived();

                _lineRenderer.SetPosition(0, laserPos);
                _lineRenderer.SetPosition(1, laserPos + (transform.forward * _maxDist));
                player.LaserRecived();
                _lastHit = null;
            }
            else if (_rayHit.collider.TryGetComponent(out ILaserReceptor receptor))
            {
                if (_lastHit != null && _lastHit != receptor)
                    _lastHit.LaserNotRecived();

                _lineRenderer.SetPosition(0, laserPos);
                _lineRenderer.SetPosition(1, _rayHit.point);
                receptor.LaserRecived();
                _lastHit = receptor;
            }
            else
            {
                if (_lastHit != null)
                {
                    _lastHit.LaserNotRecived();
                    _lastHit = null;
                }

                _lineRenderer.SetPosition(0, laserPos);
                _lineRenderer.SetPosition(1, _rayHit.point);
            }
        }
        else
        {
            if (_lastHit != null)
            {
                _lastHit.LaserNotRecived();
                _lastHit = null;
            }

            _lineRenderer.SetPosition(0, laserPos);
            _lineRenderer.SetPosition(1, laserPos + (transform.forward * _maxDist));
        }
    }

    private Vector3 GetFixedLaserPos()
    {
        return new Vector3(transform.position.x, transform.position.y, transform.position.z) + transform.forward * _raycastOffsetZ;
    }

    public void LaserRecived()
    {
        _startsInitialized = true;
    }

    public void LaserNotRecived()
    {
        Vector3 laserPos = GetFixedLaserPos();

        _startsInitialized = false;

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
