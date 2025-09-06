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
    private DefaultGlitcheable _glitcheable = null;

    private AudioSource _audioSource = default;

    private void Awake()
    {
        _glitcheable = GetComponentInParent<DefaultGlitcheable>();
        
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
        else if (!_glitcheable.IsIntargeteable)
        {
            _lineRenderer.enabled = true;
            CastLaser();
            CorruptionCheck();
        }
        else _lineRenderer.enabled = false;
    }

    #region castLaserViejo
    /*
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
                //if (_lastHit != null)
                //    _lastHit.LaserNotRecived();

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
                //if (_lastHit != null)
                //    _lastHit.LaserNotRecived();

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
                //if (_lastHit != null)
                //    _lastHit.LaserNotRecived();

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
    */
    #endregion

    #region castLaserNuevo
    private void CastLaser()
    {
        Vector3 laserPos = GetFixedLaserPos();

        _ray = new Ray(laserPos, transform.forward);
        _rightRay = new Ray(laserPos + transform.right * _raycastOffsetX, transform.forward);
        _leftRay = new Ray(laserPos - transform.right * _raycastOffsetX, transform.forward);

        if (Physics.Raycast(_ray, out _rayHit, _maxDist, _laserLayer))
        {
            // Obtenemos todos los impactos en la dirección del rayo
            RaycastHit[] hits = Physics.RaycastAll(_ray, _maxDist, _laserLayer);

            // Ordenamos por distancia (de menor a mayor)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider.TryGetComponent(out PlayerController player))
                {
                    // Cortamos el rayo en el player
                    _lineRenderer.SetPosition(0, laserPos);
                    _lineRenderer.SetPosition(1, hit.point);
                    player.LaserRecived();
                   _lastHit = player;

                    _hitLaser.transform.position = hit.point;
                    _hitLaser.transform.rotation = Quaternion.LookRotation(hit.normal);
                    if (!_hitLaser.isPlaying) _hitLaser.Play();

                    // Si querés que el láser siga "más allá del player" y pegue en la pared,
                    // sacá el "break;" de abajo
                    break;
                }
                else if (hit.collider.TryGetComponent(out ILaserReceptor receptor))
                {
                    if (_lastHit != null && _lastHit != receptor)
                        _lastHit.LaserNotRecived();

                    _lineRenderer.SetPosition(0, laserPos);
                    _lineRenderer.SetPosition(1, hit.point);
                    receptor.LaserRecived();
                    _lastHit = receptor;

                    // Partícula
                    _hitLaser.transform.position = hit.point;
                    _hitLaser.transform.rotation = Quaternion.LookRotation(hit.normal);
                    if (!_hitLaser.isPlaying) _hitLaser.Play();

                    break;
                }
                else
                {
                    // Cualquier otra cosa (pared, etc.)
                    _lineRenderer.SetPosition(0, laserPos);
                    _lineRenderer.SetPosition(1, hit.point);
                    if (_lastHit != null) _lastHit.LaserNotRecived();
                    // Partícula
                    _hitLaser.transform.position = hit.point;
                    _hitLaser.transform.rotation = Quaternion.LookRotation(hit.normal);
                    if (!_hitLaser.isPlaying) _hitLaser.Play();

                    break;
                }
            }
        }
        else
        {
            // No pegó nada => láser al máximo
            _lineRenderer.SetPosition(0, laserPos);
            _lineRenderer.SetPosition(1, laserPos + (transform.forward * _maxDist));

            if (_hitLaser.isPlaying) _hitLaser.Stop();
        }

    }
    #endregion

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
        if(!_startsInitialized)
        _isInitialized = false;

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
