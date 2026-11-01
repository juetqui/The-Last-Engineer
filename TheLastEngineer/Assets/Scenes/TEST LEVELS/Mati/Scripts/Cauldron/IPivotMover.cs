using PrimeTween;
using UnityEngine;

// Lo único que el controller necesita de quien mueve el pivot: dónde arranca y cómo pedirle un destino.
// Permite cambiar la forma de animar (otro tween, física, instantáneo) sin tocar CauldronController.
public interface IPivotMover
{
    Vector3 Position { get; }
    void MoveTo(Vector3 destination, Ease ease);
}
