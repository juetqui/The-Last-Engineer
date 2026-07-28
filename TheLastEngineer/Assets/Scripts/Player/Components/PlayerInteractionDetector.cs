using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vive en un GameObject hijo del jugador con un SphereCollider trigger + Rigidbody kinemático.
/// Detecta interactuables dentro de un radio fijo y los registra/desregistra en el
/// InteractableHandler, además de notificarles proximidad (IProximityListener).
/// Centraliza lo que antes hacían el trigger propio de cada objeto y el OnTriggerEnter/Exit
/// del PlayerController.
///
/// Es el ÚNICO dueño del registro: cualquier alta o baja tiene que pasar por acá para que el
/// set de colliders solapados y la lista del handler no se desincronicen. Si algo saca un
/// interactuable del handler por su cuenta, el interactuable queda con colliders contados pero
/// fuera de la lista y no se lo puede volver a seleccionar nunca.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerInteractionDetector : MonoBehaviour
{
    private InteractableHandler _handler;
    private PlayerController _player;
    private SphereCollider _sphere;

    // Un mismo interactuable puede tener varios colliders sólidos: guardamos el set en vez de un
    // contador para que las altas sean idempotentes y Rescan no pueda inflar el conteo.
    private readonly Dictionary<IInteractable, HashSet<Collider>> _overlaps = new();

    public float Radius
    {
        get
        {
            if (_sphere == null) return 0f;

            Vector3 scale = transform.lossyScale;
            float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

            return _sphere.radius * maxScale;
        }
    }

    private void Awake()
    {
        _sphere = GetComponent<SphereCollider>();
    }

    public void Initialize(InteractableHandler handler, PlayerController player, float radius = 0f)
    {
        _handler = handler;
        _player = player;

        if (_sphere == null) _sphere = GetComponent<SphereCollider>();
        if (radius > 0f) _sphere.radius = radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable == null) return;

        Track(interactable, other);
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable == null) return;

        Untrack(interactable, other);
    }

    /// <summary>
    /// Vuelve a evaluar la pertenencia de un interactuable al radio sin depender de que PhysX
    /// re-emita OnTriggerEnter al reactivar un collider que ya estaba dentro del trigger.
    /// Lo usa NodeController al soltarse: con autoSyncTransforms desactivado ese enter es el
    /// único eslabón del ciclo soltar -> volver a levantar, y no conviene depender de él.
    /// </summary>
    public void Rescan(IInteractable interactable, Collider coll)
    {
        if (_handler == null || interactable == null || coll == null) return;

        bool inRange = coll.enabled
            && coll.gameObject.activeInHierarchy
            && (coll.transform.position - transform.position).sqrMagnitude <= Radius * Radius;

        if (inRange) Track(interactable, coll);
        else Untrack(interactable, coll);
    }

    /// <summary>
    /// Baja explícita: saca el interactuable del set Y de la lista. Es lo que tienen que usar
    /// los estados del jugador cuando "consumen" un interactuable (levantar un nodo, usar una
    /// plataforma), en lugar de tocar el InteractableHandler directamente.
    /// </summary>
    public void Forget(IInteractable interactable)
    {
        if (interactable == null) return;

        bool wasTracked = _overlaps.Remove(interactable);

        _handler?.Remove(interactable);

        if (wasTracked) (interactable as IProximityListener)?.OnPlayerProximity(false, _player);
    }

    /// <summary>
    /// Limpieza total. Va de la mano de InteractableHandler.Clear(): si se limpia la lista sin
    /// limpiar el set, todo lo que siga solapado queda registrado como "ya contado" y no vuelve
    /// a entrar a la lista nunca (caso: morir y reaparecer dentro del radio de un nodo).
    /// </summary>
    public void ResetTracking()
    {
        foreach (var pair in _overlaps)
            (pair.Key as IProximityListener)?.OnPlayerProximity(false, _player);

        _overlaps.Clear();
    }

    private void Track(IInteractable interactable, Collider coll)
    {
        if (_handler == null) return;

        bool wasEmpty = !_overlaps.TryGetValue(interactable, out var colliders);

        if (wasEmpty)
        {
            colliders = new HashSet<Collider>();
            _overlaps[interactable] = colliders;
        }

        colliders.Add(coll);

        // Add ya chequea duplicados, así que llamarlo siempre sirve de red de seguridad ante
        // cualquier desincronización previa entre el set y la lista.
        _handler.Add(interactable);

        // La notificación solo en la transición fuera -> dentro: hay interactuables que
        // reaccionan con feedback no idempotente (PlatformTeleport reinicia su ParticleSystem).
        if (wasEmpty) (interactable as IProximityListener)?.OnPlayerProximity(true, _player);
    }

    private void Untrack(IInteractable interactable, Collider coll)
    {
        if (_handler == null) return;
        if (!_overlaps.TryGetValue(interactable, out var colliders)) return;

        colliders.Remove(coll);
        if (colliders.Count > 0) return;

        _overlaps.Remove(interactable);
        _handler.Remove(interactable);
        (interactable as IProximityListener)?.OnPlayerProximity(false, _player);
    }
}
