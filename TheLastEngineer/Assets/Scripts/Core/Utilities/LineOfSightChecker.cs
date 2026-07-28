using UnityEngine;

/// <summary>
/// Chequea si hay un obstáculo (por ejemplo una pared) entre el jugador y un interactuable.
/// Reutiliza el patrón de CutoutObject / PlayerController.CheckForWalls: eleva el origen en Y
/// para no arrancar el rayo dentro del piso, e ignora colliders trigger.
///
/// La regla de auto-exclusión es el punto delicado: se descartan únicamente los impactos que
/// pertenecen al SUBÁRBOL del objetivo, no a su raíz. Un interactuable no puede taparse a sí
/// mismo, pero la geometría de su padre sí tiene que taparlo:
/// - En Wall_Connection la pared (layer Walls) es el PADRE del objeto que lleva el componente
///   Connection. Descartar toda la raíz dejaba conectar desde el otro lado de la pared.
/// - Connection_Pared / Connection_Piso llevan el componente en su propia raíz de prefab, pero en
///   escena cuelgan de módulos de nivel: filtrar por raíz ignoraba TODAS las paredes del módulo.
///
/// El segmento se acorta un margen antes del objetivo para que el extremo no roce la superficie
/// sobre la que el interactuable está montado (caso tangente, que si no resuelve el error de
/// punto flotante y por lo tanto es inestable).
/// </summary>
public class LineOfSightChecker : IObstructionChecker
{
    private const int MaxHits = 8;

    private readonly LayerMask _obstacleMask;
    private readonly float _heightOffset;
    private readonly float _endMargin;
    private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];

    public float HeightOffset => _heightOffset;

    public LineOfSightChecker(LayerMask obstacleMask, float heightOffset = 2f, float endMargin = 0.25f)
    {
        _obstacleMask = obstacleMask;
        _heightOffset = heightOffset;
        _endMargin = endMargin;
    }

    public bool IsObstructed(Vector3 from, Transform target)
        => target != null && IsObstructed(from, target, DefaultAimPoint(target));

    /// <summary>
    /// Versión con punto de mira explícito: el llamador pasa el centro del collider sólido del
    /// interactuable en vez de su pivote. Los pivotes de nodos y conexiones están al ras del piso
    /// (el de Wall_Connection cae justo sobre el plano de la cara de la pared), así que apuntar al
    /// pivote hace que el rayo roce geometría que no debería tocar.
    /// </summary>
    public bool IsObstructed(Vector3 from, Transform target, Vector3 aimPoint)
    {
        if (target == null) return false;

        Vector3 origin = from + Vector3.up * _heightOffset;
        Vector3 delta = aimPoint - origin;

        float distance = delta.magnitude - _endMargin;
        if (distance <= 0f) return false;

        int count = Physics.RaycastNonAlloc(origin, delta.normalized, _hits, distance, _obstacleMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            // IsChildOf devuelve true también para el propio transform, así que cubre el objetivo
            // y todo lo que cuelgue de él, sin alcanzar a padres ni hermanos.
            if (_hits[i].transform.IsChildOf(target)) continue;

            return true;
        }

        return false;
    }

    private Vector3 DefaultAimPoint(Transform target) => target.position + Vector3.up * _heightOffset;
}
