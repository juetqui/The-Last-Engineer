using UnityEngine;

/// <summary>Acci�n activa mientras se sostiene (ej. linterna, orbe que altera el entorno).</summary>
public interface IUsable
{
    /// <summary>Se activa cuando el jugador presiona �usar� y el objeto est� en mano.</summary>
    void UseStart(GameObject user);
    /// <summary>Se llama cada frame mientras se mantiene la acci�n de uso.</summary>
    void UseTick(GameObject user, float dt);
    /// <summary>Se suelta el bot�n de uso.</summary>
    void UseStop(GameObject user);
}