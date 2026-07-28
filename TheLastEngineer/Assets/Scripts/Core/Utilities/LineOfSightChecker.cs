using UnityEngine;

/// <summary>
/// Chequea si hay un obstáculo (por ejemplo una pared) entre el jugador y un interactuable.
/// Reutiliza el patrón de CutoutObject / PlayerController.CheckForWalls: eleva los extremos
/// en Y para no arrancar el rayo dentro del piso, e ignora colliders trigger.
///
/// Dos detalles importantes para que no de falsos positivos:
/// - Descarta los impactos que pertenecen a la jerarquía del propio objetivo. El prefab
///   Wall_Connection tiene la pared (layer Walls, = wallMask) como PADRE del objeto que lleva
///   el componente Connection (layer Default), así que sin este filtro la conexión quedaría
///   permanentemente tapada por su propia pared.
/// - Acorta el segmento un margen antes del objetivo, para que el extremo no roce la
///   superficie sobre la que el interactuable está montado (caso tangente, que hoy resuelve
///   el error de punto flotante y por lo tanto es inestable).
/// </summary>
public class LineOfSightChecker : IObstructionChecker
{
    private const int MaxHits = 8;

    private readonly LayerMask _obstacleMask;
    private readonly float _heightOffset;
    private readonly float _endMargin;
    private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];

    public float HeightOffset => _heightOffset;

    public LineOfSightChecker(LayerMask obstacleMask, float heightOffset = 2f, float endMargin = 0.5f)
    {
        _obstacleMask = obstacleMask;
        _heightOffset = heightOffset;
        _endMargin = endMargin;
    }

    public bool IsObstructed(Vector3 from, Transform target)
    {
        if (target == null) return false;

        Vector3 offset = Vector3.up * _heightOffset;
        Vector3 origin = from + offset;
        Vector3 delta = (target.position + offset) - origin;

        float distance = delta.magnitude - _endMargin;
        if (distance <= 0f) return false;

        Transform targetRoot = target.root;
        int count = Physics.RaycastNonAlloc(origin, delta.normalized, _hits, distance, _obstacleMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (_hits[i].transform.root == targetRoot) continue;

            return true;
        }

        return false;
    }
}
