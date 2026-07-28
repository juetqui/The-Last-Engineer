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
///
/// También es el dueño del chequeo de línea de visión: lo evalúa por frame para todo lo que está
/// solapado y publica el resultado en el handler. El estado efectivo de un interactuable es
/// "solapado Y visible", y es ese estado (no el solapamiento crudo) el que se notifica como
/// proximidad, así el feedback del objeto nunca contradice lo que el botón puede hacer.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerInteractionDetector : MonoBehaviour
{
    /// <summary>
    /// Estado por interactuable solapado. Un mismo interactuable puede tener varios colliders
    /// sólidos: guardamos el set en vez de un contador para que las altas sean idempotentes y
    /// Rescan no pueda inflar el conteo.
    /// </summary>
    private class Tracked
    {
        public readonly HashSet<Collider> Colliders = new();
        public bool Visible;
        public float PendingTime;
    }

    private InteractableHandler _handler;
    private PlayerController _player;
    private IObstructionChecker _obstruction;
    private SphereCollider _sphere;

    private float _checkInterval;
    private float _debounceTime;
    private float _lastEvalTime;

    private readonly Dictionary<IInteractable, Tracked> _overlaps = new();

    // Buffers reutilizados por el barrido: las notificaciones se despachan DESPUÉS de recorrer el
    // diccionario porque un listener puede terminar llamando a Forget y mutar la colección.
    private readonly List<IInteractable> _stale = new();
    private readonly List<(IInteractable interactable, bool visible)> _transitions = new();

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

    public void Initialize(InteractableHandler handler, PlayerController player, IObstructionChecker obstruction,
        float radius = 0f, float checkInterval = 0f, float debounceTime = 0f)
    {
        _handler = handler;
        _player = player;
        _obstruction = obstruction;
        _checkInterval = checkInterval;
        _debounceTime = debounceTime;
        _lastEvalTime = Time.time;

        if (_sphere == null) _sphere = GetComponent<SphereCollider>();
        if (radius > 0f) _sphere.radius = radius;
    }

    /// <summary>
    /// Barrido de línea de visión. Corre por frame (o cada checkInterval) sobre lo que está
    /// solapado, así el estado "tapado por una pared" se marca en el momento en que la pared se
    /// cruza y no recién cuando el jugador aprieta el botón.
    /// </summary>
    private void Update()
    {
        if (_handler == null || _obstruction == null) return;

        float elapsed = Time.time - _lastEvalTime;

        if (_checkInterval > 0f && elapsed < _checkInterval) return;

        _lastEvalTime = Time.time;

        if (_overlaps.Count == 0) return;

        Vector3 origin = _player != null ? _player.transform.position : transform.position;

        foreach (var pair in _overlaps)
        {
            var interactable = pair.Key;

            if (IsDestroyed(interactable))
            {
                _stale.Add(interactable);
                continue;
            }

            var tracked = pair.Value;
            bool visible = !IsObstructed(origin, interactable, tracked);

            if (visible == tracked.Visible)
            {
                tracked.PendingTime = 0f;
                continue;
            }

            // Debounce en ambos sentidos: rozar una columna al correr hace oscilar el rayo, y
            // cada transición a visible reinicia feedback no idempotente (las partículas de
            // PlatformTeleport, el tween de escala del orbit del Glitcheable).
            tracked.PendingTime += elapsed;
            if (tracked.PendingTime < _debounceTime) continue;

            tracked.PendingTime = 0f;
            tracked.Visible = visible;

            _handler.SetLineOfSight(interactable, visible);
            _transitions.Add((interactable, visible));
        }

        foreach (var interactable in _stale)
        {
            _overlaps.Remove(interactable);
            _handler.Remove(interactable);
        }

        _stale.Clear();

        foreach (var (interactable, visible) in _transitions)
            (interactable as IProximityListener)?.OnPlayerProximity(visible, _player);

        _transitions.Clear();
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

        bool wasVisible = _overlaps.TryGetValue(interactable, out var tracked) && tracked.Visible;

        _overlaps.Remove(interactable);
        _handler?.Remove(interactable);

        if (wasVisible) (interactable as IProximityListener)?.OnPlayerProximity(false, _player);
    }

    /// <summary>
    /// Limpieza total. Va de la mano de InteractableHandler.Clear(): si se limpia la lista sin
    /// limpiar el set, todo lo que siga solapado queda registrado como "ya contado" y no vuelve
    /// a entrar a la lista nunca (caso: morir y reaparecer dentro del radio de un nodo).
    /// </summary>
    public void ResetTracking()
    {
        // Se vacía el set ANTES de notificar: un listener puede terminar llamando a Forget y
        // mutar el diccionario en medio del recorrido.
        foreach (var pair in _overlaps)
        {
            if (pair.Value.Visible && !IsDestroyed(pair.Key)) _stale.Add(pair.Key);
        }

        _overlaps.Clear();

        foreach (var interactable in _stale)
            (interactable as IProximityListener)?.OnPlayerProximity(false, _player);

        _stale.Clear();
    }

    private void Track(IInteractable interactable, Collider coll)
    {
        if (_handler == null) return;

        bool isNew = !_overlaps.TryGetValue(interactable, out var tracked);

        if (isNew)
        {
            tracked = new Tracked();
            _overlaps[interactable] = tracked;
        }

        tracked.Colliders.Add(coll);

        // Add ya chequea duplicados, así que llamarlo siempre sirve de red de seguridad ante
        // cualquier desincronización previa entre el set y la lista.
        _handler.Add(interactable);

        if (!isNew) return;

        // Evaluamos el LOS en el alta en vez de asumir visible: si el objeto entra al radio ya
        // tapado, queda registrado pero bloqueado y no dispara feedback que el jugador no puede usar.
        Vector3 origin = _player != null ? _player.transform.position : transform.position;

        tracked.Visible = !IsObstructed(origin, interactable, tracked);
        tracked.PendingTime = 0f;

        _handler.SetLineOfSight(interactable, tracked.Visible);

        // La notificación solo en la transición fuera -> dentro: hay interactuables que
        // reaccionan con feedback no idempotente (PlatformTeleport reinicia su ParticleSystem).
        if (tracked.Visible) (interactable as IProximityListener)?.OnPlayerProximity(true, _player);
    }

    private void Untrack(IInteractable interactable, Collider coll)
    {
        if (_handler == null) return;
        if (!_overlaps.TryGetValue(interactable, out var tracked)) return;

        tracked.Colliders.Remove(coll);
        if (tracked.Colliders.Count > 0) return;

        _overlaps.Remove(interactable);
        _handler.Remove(interactable);

        // Si ya estaba tapado, su feedback está apagado desde que se cruzó la pared: avisar de
        // nuevo dispararía un apagado duplicado.
        if (tracked.Visible) (interactable as IProximityListener)?.OnPlayerProximity(false, _player);
    }

    private bool IsObstructed(Vector3 origin, IInteractable interactable, Tracked tracked)
    {
        return TryGetAimPoint(origin, tracked, out var aimPoint)
            ? _obstruction.IsObstructed(origin, interactable.Transform, aimPoint)
            : _obstruction.IsObstructed(origin, interactable.Transform);
    }

    /// <summary>
    /// Punto de mira = centro del collider SÓLIDO trackeado más cercano al jugador. Es más fiel
    /// que el pivote (que en nodos y conexiones está al ras del piso) y sale de datos que ya
    /// tenemos. Los triggers quedan afuera: varios prefabs todavía arrastran el trigger grande que
    /// usaban para detectar al jugador antes de este refactor, y su centro está desplazado
    /// respecto del objeto real (en Connection_Pared, casi medio metro hacia afuera).
    /// </summary>
    private bool TryGetAimPoint(Vector3 origin, Tracked tracked, out Vector3 aimPoint)
    {
        aimPoint = Vector3.zero;

        float closest = float.MaxValue;

        foreach (var coll in tracked.Colliders)
        {
            if (coll == null || coll.isTrigger || !coll.enabled || !coll.gameObject.activeInHierarchy) continue;

            Vector3 center = coll.bounds.center;
            float sqrDist = (center - origin).sqrMagnitude;

            if (sqrDist >= closest) continue;

            closest = sqrDist;
            aimPoint = center;
        }

        return closest < float.MaxValue;
    }

    // Los interactuables son MonoBehaviours: si el objeto fue destruido, tocar su Transform tira
    // MissingReferenceException, así que el chequeo va contra el operador == de UnityEngine.Object.
    private static bool IsDestroyed(IInteractable interactable)
        => interactable is UnityEngine.Object obj && obj == null;
}
