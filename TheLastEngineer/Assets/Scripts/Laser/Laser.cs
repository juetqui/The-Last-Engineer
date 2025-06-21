using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _maxDist = 20f;
    [SerializeField] private bool _startsInitialized = false;
    [SerializeField] private LayerMask _laserLayer;

    private ILaserReceptor _lastHit = null;
    private RaycastHit _rayHit;
    private Ray _ray;

    private void Awake()
    {
        _lineRenderer.positionCount = 2;

        Vector3 laserPos = GetFixedLaserPos();

        _lineRenderer.SetPosition(0, laserPos);
        _lineRenderer.SetPosition(1, laserPos);
    }

    private void Start()
    {
        if (_startsInitialized) CastLaser();
    }

    private void Update()
    {
        if (!_startsInitialized) return;

        CastLaser();
        CorruptionCheck();
    }

    private void CastLaser()
    {
        Vector3 laserPos = GetFixedLaserPos();

        _ray = new Ray(laserPos, transform.forward);

        if (Physics.Raycast(_ray, out _rayHit, _maxDist, _laserLayer))
        {
            if (_rayHit.collider.TryGetComponent(out PlayerTDController player))
            {
                if (_lastHit != null && player != _lastHit)
                {
                    _lastHit.LaserNotRecived();
                }

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
        return new Vector3(transform.position.x, transform.position.y, transform.position.z) + transform.forward * 2;
    }

    public void LaserRecived()
    {
        _startsInitialized = true;
        CastLaser();
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
}
