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
            _cutoutMat.SetFloat("_CutoutSize", 0.1f);
            _cutoutMat.SetFloat("_FalloffSize", 0.05f);
            _cutoutMat.SetFloat("_EnableCutout", 1f);
        }
        else
            _cutoutMat.SetFloat("_EnableCutout", 0f);
    }

    private void OnDrawGizmos()
    {
        if (_target == null)
            return;

        // Si ya tenés la información de colisión
        if (_hasObstacle)
        {
            // Línea hasta el punto de impacto (roja)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _hit.point);

            // Punto de impacto
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_hit.point, 0.05f);
        }
        else
        {
            // Si no hay colisión, dibujá toda la línea en verde
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _target.position);
        }

        // Opcional: dibujar esferas en el origen y destino
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.03f);
        Gizmos.DrawSphere(_target.position, 0.03f);
    }
}
