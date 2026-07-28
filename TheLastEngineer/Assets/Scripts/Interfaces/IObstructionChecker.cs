using UnityEngine;

/// <summary>
/// Abstracción para chequear si hay un obstáculo entre el jugador y un objetivo.
/// Permite inyectar la lógica de línea de visión sin acoplar a Physics directamente (DIP).
/// Recibe el Transform del objetivo (y no un Vector3) para poder descartar la geometría de su
/// propia jerarquía: una Connection montada sobre una pared es hija de esa pared, que está en
/// la wallMask y por lo tanto se bloquearía a sí misma.
/// </summary>
public interface IObstructionChecker
{
    bool IsObstructed(Vector3 from, Transform target);

    /// <summary>
    /// Igual que el anterior pero apuntando a un punto concreto del objetivo (típicamente el
    /// centro de su collider) en lugar de a su pivote. El pivote de la mayoría de los
    /// interactuables está al ras del piso y no representa dónde está el objeto realmente.
    /// </summary>
    bool IsObstructed(Vector3 from, Transform target, Vector3 aimPoint);
}
