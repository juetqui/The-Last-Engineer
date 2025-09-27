using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableHandler
{
    private readonly List<IInteractable> _interactables = new();

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

        return _interactables.Where(i => i.CanInteract(nodeHandler))
            .OrderBy(i => i.Priority)
            .OrderBy(i => Vector3.Distance(i.Transform.position, playerPos))
            .FirstOrDefault();
    }

    public Glitcheable GetClosestGlitcheable(Vector3 playerPos)
    {
        if (_interactables.Count <= 0) return null;

        return _interactables.OfType<Glitcheable>().OrderBy(i => Vector3.Distance(i.transform.position, playerPos)).FirstOrDefault();
    }
}
