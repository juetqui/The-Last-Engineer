using System;
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

    /// <summary>
    /// ¿Se lo puede seguir eligiendo? Suma a la línea de visión el seguir en la lista: el detector
    /// da de baja lo que se va del radio aunque PhysX no haya emitido el OnTriggerExit (el
    /// Glitcheable apaga sus colliders y se mueve, y ese evento nunca llega).
    /// </summary>
    public bool IsSelectable(IInteractable it) => HasLineOfSight(it) && _interactables.Contains(it);

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

    /// <summary>
    /// The closest visible interactable of type T. Set and Take use the optional filter to skip the
    /// ones that can't take part in the swap: without it, with two objects in range, the swap would
    /// fail on the right one just because the other was a few centimeters closer.
    /// A loop and not LINQ: the HUD calls this every frame.
    /// </summary>
    public T GetClosest<T>(Vector3 playerPos, Func<T, bool> filter = null) where T : class, IInteractable
    {
        T closest = null;
        float closestSqr = float.MaxValue;

        for (int i = 0; i < _interactables.Count; i++)
        {
            if (_interactables[i] is not T candidate) continue;
            if (candidate is UnityEngine.Object obj && obj == null) continue;
            if (!HasLineOfSight(candidate)) continue;
            if (filter != null && !filter(candidate)) continue;

            float sqr = (candidate.Transform.position - playerPos).sqrMagnitude;
            if (sqr >= closestSqr) continue;

            closest = candidate;
            closestSqr = sqr;
        }

        return closest;
    }
}
