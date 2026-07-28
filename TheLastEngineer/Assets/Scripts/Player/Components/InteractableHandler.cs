using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Lista de interactuables al alcance del jugador, con un flag de línea de visión por ítem.
/// No calcula el LOS: lo hace PlayerInteractionDetector una vez por frame y lo publica acá con
/// SetLineOfSight. Las consultas (selección al apretar, glitcheable más cercano) leen el flag
/// cacheado, así el resultado del botón es exactamente lo que el jugador está viendo en pantalla.
/// </summary>
public class InteractableHandler
{
    private readonly List<IInteractable> _interactables = new();

    // Guardamos los tapados y no los visibles: un interactuable recién registrado arranca visible
    // (nadie lo bloqueó todavía) y no hay ventana de un frame en la que no se pueda interactuar.
    private readonly HashSet<IInteractable> _blocked = new();

    // Solo lectura para herramientas de debug (dibujo de gizmos, etc.).
    public IReadOnlyList<IInteractable> Interactables => _interactables;

    public void Add(IInteractable it)
    {
        if (it != null && !_interactables.Contains(it))
            _interactables.Add(it);
    }

    public void Remove(IInteractable it)
    {
        if (it == null) return;

        _interactables.Remove(it);
        _blocked.Remove(it);
    }

    public void Clear()
    {
        _interactables.Clear();
        _blocked.Clear();
    }

    public void SetLineOfSight(IInteractable it, bool visible)
    {
        if (it == null) return;

        if (visible) _blocked.Remove(it);
        else _blocked.Add(it);
    }

    public bool HasLineOfSight(IInteractable it) => it != null && !_blocked.Contains(it);

    public IInteractable GetInteractable(PlayerNodeHandler nodeHandler, Vector3 playerPos)
    {
        if (_interactables.Count <= 0) return null;

        // OrderByDescending porque el enum va de menor a mayor importancia (Low = 0 ... MaxPriority = 4):
        // gana la prioridad más alta y, a igualdad de prioridad, el más cercano.
        return _interactables.Where(i => i.CanInteract(nodeHandler) && HasLineOfSight(i))
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => Vector3.Distance(i.Transform.position, playerPos))
            .FirstOrDefault();
    }

    public Glitcheable GetClosestGlitcheable(Vector3 playerPos)
    {
        if (_interactables.Count <= 0) return null;

        return _interactables.OfType<Glitcheable>()
            .Where(i => HasLineOfSight(i))
            .OrderBy(i => Vector3.Distance(i.transform.position, playerPos))
            .FirstOrDefault();
    }
}
