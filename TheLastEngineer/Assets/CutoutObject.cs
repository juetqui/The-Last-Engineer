using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Material _cutoutMat;
    [SerializeField] private LayerMask _layerMask;

    private Camera _mainCamera = default;
    private RaycastHit _hit;
    private bool _hasObstacle;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        CheckCutout();
    }

    private void CheckCutout()
    {
        Vector3 offset = _target.position - transform.position;

        _hasObstacle = Physics.Linecast(transform.position, _target.position, out _hit, _layerMask);

        if (_hasObstacle)
        {
            Vector3 cutoutPos = _mainCamera.WorldToViewportPoint(_hit.point);
            cutoutPos.y /= (Screen.width / Screen.height);

            _cutoutMat.SetVector("_CutoutPos", cutoutPos);
            _cutoutMat.SetFloat("_EnableCutout", 1f);
        }
        else
            _cutoutMat.SetFloat("_EnableCutout", 0f);
    }
}
