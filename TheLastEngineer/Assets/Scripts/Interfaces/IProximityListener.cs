/// <summary>
/// Interactuables que quieren feedback cuando el jugador entra o sale de su rango.
/// El PlayerInteractionDetector invoca esto en lugar del OnTriggerEnter/Exit propio del objeto,
/// centralizando la detección de proximidad en el trigger fijo del jugador (ISP).
/// </summary>
public interface IProximityListener
{
    void OnPlayerProximity(bool inRange, PlayerController player);
}
