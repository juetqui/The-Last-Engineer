using UnityEngine;

/// <summary>Algo que el jugador puede agarrar, sostener y soltar.</summary>
public interface IGrabbable
{
    bool TryGrab(Transform holder);
    bool TryRelease();
    Transform Transform { get; }
}
