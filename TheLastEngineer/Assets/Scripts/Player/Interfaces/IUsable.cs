using UnityEngine;

/// <summary>Acción activa mientras se sostiene (ej. linterna, orbe que altera el entorno).</summary>
public interface IUsable
{
    /// <summary>Se activa cuando el jugador presiona “usar” y el objeto está en mano.</summary>
    void UseStart(GameObject user);
    /// <summary>Se llama cada frame mientras se mantiene la acción de uso.</summary>
    void UseTick(GameObject user, float dt);
    /// <summary>Se suelta el botón de uso.</summary>
    void UseStop(GameObject user);
}