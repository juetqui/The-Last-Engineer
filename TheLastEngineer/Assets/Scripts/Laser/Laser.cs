using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private string _reflectableTag;
    [SerializeField] private int _maxBounces;
    [SerializeField] private float _maxDist;
    [SerializeField] private bool _onlyReflectables;
    public Action _onCollition;
    PlayerTDController _playerTDController;

   public GameObject objectsHits;

    private LineRenderer _lineRenderer = default;
    private bool _isResetting = false;

    private void Start()
    {
        _playerTDController = FindObjectOfType<PlayerTDController>();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.SetPosition(0, _startPoint.position);

    }

    private void Update()
    {
        if (_isResetting) return;

        CastLaser(_startPoint.position, _startPoint.forward);
        CorruptionCheck();
    }

    private void CastLaser(Vector3 position, Vector3 direction)
    {
        _lineRenderer.SetPosition(0, _startPoint.position);

        for (int i = 0; i < _maxBounces; i++)
        {
            direction = new Vector3(direction.x, /*direction.y*/0, direction.z);

            Ray ray = new Ray(position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _maxDist))
            {
                if (hit.transform.TryGetComponent(out PlayerTDController player))
                {
                    player.LaserCollition();
                    position = hit.point;
                    _lineRenderer.SetPosition(i + 1, position);

                    continue;
                }
                
                if (hit.transform.TryGetComponent(out ILaserReceptor laserReceptor))
                {
                    if (objectsHits != null && objectsHits != hit.transform.gameObject)
                    {
                        objectsHits.GetComponent<ILaserReceptor>().LaserNotRecived();
                        laserReceptor.LaserRecived();
                        objectsHits = hit.transform.gameObject;
                    }
                    else
                    {
                        objectsHits = hit.transform.gameObject;
                        laserReceptor.LaserRecived();
                    }
                }
                else if(!hit.transform.gameObject.CompareTag(_reflectableTag) && objectsHits != null)
                {
                    objectsHits.GetComponent<ILaserReceptor>().LaserNotRecived();
                    objectsHits = default;
                }

                position = hit.point;
                direction = Vector3.Reflect(direction, hit.normal);

                _lineRenderer.SetPosition(i + 1, position);

                if (hit.transform.tag != _reflectableTag && _onlyReflectables)
                {
                    for (int j = (i + 1); j <= _maxBounces; j++)
                    {
                        _lineRenderer.SetPosition(j, position);
                    }

                    break;
                }
                
                if (CollitionCheck(hit)) _onCollition();
            }
        }
    }
    
    protected virtual void CorruptionCheck()
    {

    }

    protected virtual bool CollitionCheck(RaycastHit hit)
    {
        return default;
    }
}
