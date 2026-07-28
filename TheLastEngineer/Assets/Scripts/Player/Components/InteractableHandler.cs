using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableHandler
{
    private readonly List<IInteractable> _interactables = new();
    private readonly IObstructionChecker _obstruction;

    // Solo lectura para herramientas de debug (dibujo de gizmos, etc.).
    public IReadOnlyList<IInteractable> Interactables => _interactables;

    public InteractableHandler(IObstructionChecker obstruction = null)
    {
        _obstruction = obstruction;
    }

    public void Add(IInteractable it)
    {
        if (it != null && !_interactables.Contains(it))
            _interactables.Add(it);
    }

    public void Remove(IInteractable it)
    {
        if (it != null) _interactables.Remove(it);
    }

    public void Clear() => _interactables.Clear();

    public IInteractable GetInteractable(PlayerNodeHandler nodeHandler, Vector3 playerPos)
    {
        if (_interactables.Count <= 0) return null;

        // OrderByDescending porque el enum va de menor a mayor importancia (Low = 0 ... MaxPriority = 4):
        // gana la prioridad más alta y, a igualdad de prioridad, el más cercano.
        return _interactables.Where(i => i.CanInteract(nodeHandler) && HasLineOfSight(i, playerPos))
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => Vector3.Distance(i.Transform.position, playerPos))
            .FirstOrDefault();
    }

    public Glitcheable GetClosestGlitcheable(Vector3 playerPos)
    {
        if (_interactables.Count <= 0) return null;

        return _interactables.OfType<Glitcheable>()
            .Where(i => HasLineOfSight(i, playerPos))
            .OrderBy(i => Vector3.Distance(i.transform.position, playerPos))
            .FirstOrDefault();
    }

    // Gate de línea de visión continuo: si no hay checker inyectado, no filtra nada.
    private bool HasLineOfSight(IInteractable interactable, Vector3 playerPos)
        => _obstruction == null || !_obstruction.IsObstructed(playerPos, interactable.Transform);
}
