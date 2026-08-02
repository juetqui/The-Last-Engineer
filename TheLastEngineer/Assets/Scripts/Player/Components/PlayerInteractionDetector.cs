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
/// Los eventos de PhysX se usan para DESCUBRIR interactuables, no como fuente de verdad: hay
/// varias formas de dejar de solapar sin que se emita OnTriggerExit (apagar el collider, cambiar
/// de layer, teletransportarse con autoSyncTransforms desactivado), y el ciclo del Glitcheable
/// hace las tres. Por eso el barrido de Update revalida cada entrada contra el estado real de sus
/// colliders en lugar de esperar el evento de salida.
///
/// También es el dueño del chequeo de línea de visión: lo evalúa por frame para todo lo que está
/// solapado y publica el resultado en el handler. El estado efectivo de un interactuable es
/// "presente Y visible", y es ese estado (no el solapamiento crudo) el que se notifica como
/// proximidad, así el feedback del objeto nunca contradice lo que el botón puede hacer.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerInteractionDetector : MonoBehaviour
{
    /// <summary>
    /// Estado por interactuable detectado. Un mismo interactuable puede tener varios colliders
    /// sólidos: guardamos el set en vez de un contador para que las altas sean idempotentes y
    /// Rescan no pueda inflar el conteo.
    ///
    /// Registered distingue "lo tenemos fichado" de "está en la lista de interactuables". Una
    /// entrada puede quedar fichada pero sin registrar (dormida) cuando el objeto apaga sus
    /// colliders: así conservamos las referencias para volver a registrarlo si reaparece dentro
    /// del radio, sin depender de que PhysX emita un enter nuevo.
    /// </summary>
    private class Tracked
    {
        public readonly HashSet<Collider> Colliders = new();
        public bool Registered;
        public bool Visible;
        public float PendingTime;
    }

    // El barrido tiene que ser MÁS permisivo que PhysX: si diéramos de baja algo que PhysX todavía
    // considera solapado, no llegaría ningún enter nuevo y el interactuable quedaría perdido hasta
    // salir y volver a entrar. Por eso el test de rango usa el AABB (más grande que la geometría
    // real) y encima un margen.
    private const float RangeTolerance = 1.1f;

    private static readonly System.Predicate<Collider> IsMissing = coll => coll == null;

    private InteractableHandler _handler;
    private PlayerController _player;
    private IObstructionChecker _obstruction;
    private SphereCollider _sphere;

    private float _checkInterval;
    private float _debounceTime;
    private float _lastEvalTime;

    private readonly Dictionary<IInteractable, Tracked> _overlaps = new();

    // Buffers reutilizados por el barrido: las bajas y las notificaciones se aplican DESPUÉS de
    // recorrer el diccionario porque un listener puede terminar llamando a Forget y mutar la colección.
    private readonly List<IInteractable> _dropped = new();
    private readonly List<(IInteractable interactable, bool visible)> _transitions = new();
    private bool _dispatching;

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

    private Vector3 Center => _sphere == null ? transform.position : transform.TransformPoint(_sphere.center);

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
    /// Barrido por frame (o cada checkInterval). Hace dos cosas sobre lo que tenemos fichado:
    /// revalida la presencia real contra el estado de los colliders, y recalcula la línea de
    /// visión, así "tapado por una pared" se marca en el momento en que la pared se cruza y no
    /// recién cuando el jugador aprieta el botón.
    /// </summary>
    private void Update()
    {
        if (_handler == null || _obstruction == null) return;

        float elapsed = Time.time - _lastEvalTime;

        if (_checkInterval > 0f && elapsed < _checkInterval) return;

        _lastEvalTime = Time.time;

        if (_overlaps.Count == 0) return;

        Vector3 center = Center;
        Vector3 origin = _player != null ? _player.transform.position : center;

        foreach (var pair in _overlaps)
        {
            var interactable = pair.Key;
            var tracked = pair.Value;

            if (IsDestroyed(interactable))
            {
                _dropped.Add(interactable);
                continue;
            }

            bool present = IsPresent(tracked, center, out bool hasLiveCollider);

            if (!present)
            {
                if (tracked.Registered) Unregister(interactable, tracked);

                // Sin colliders vivos el objeto está dormido (el Glitcheable apaga los suyos
                // mientras se desintegra y se mueve): mantenemos la ficha para poder darlo de alta
                // solo si vuelve dentro del radio. Si ya tiene colliders vivos y aun así no está
                // presente, se fue de verdad.
                if (hasLiveCollider || tracked.Colliders.Count == 0) _dropped.Add(interactable);

                continue;
            }

            if (!tracked.Registered)
            {
                Register(interactable, tracked, origin);
                continue;
            }

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

        foreach (var interactable in _dropped)
            _overlaps.Remove(interactable);

        _dropped.Clear();

        Dispatch();
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

        // Mismo criterio de presencia que el barrido, para que las dos vías no se contradigan y
        // un objeto que reactiva su collider lejos del jugador no dé de alta por un frame.
        float limit = Radius * RangeTolerance;

        bool present = coll.enabled
            && coll.gameObject.activeInHierarchy
            && coll.bounds.SqrDistance(Center) <= limit * limit;

        if (present) Track(interactable, coll);
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

        if (_overlaps.TryGetValue(interactable, out var tracked))
        {
            _overlaps.Remove(interactable);
            Unregister(interactable, tracked);
        }
        else _handler?.Remove(interactable);

        Dispatch();
    }

    /// <summary>
    /// Limpieza total. Va de la mano de InteractableHandler.Clear(): si se limpia la lista sin
    /// limpiar el set, todo lo que siga solapado queda registrado como "ya contado" y no vuelve
    /// a entrar a la lista nunca (caso: morir y reaparecer dentro del radio de un nodo).
    /// </summary>
    public void ResetTracking()
    {
        foreach (var pair in _overlaps)
        {
            if (pair.Value.Visible && !IsDestroyed(pair.Key)) _transitions.Add((pair.Key, false));
        }

        _overlaps.Clear();

        Dispatch();
    }

    private void Track(IInteractable interactable, Collider coll)
    {
        if (_handler == null) return;

        if (!_overlaps.TryGetValue(interactable, out var tracked))
        {
            tracked = new Tracked();
            _overlaps[interactable] = tracked;
        }

        tracked.Colliders.Add(coll);

        if (tracked.Registered)
        {
            // Add ya chequea duplicados, así que llamarlo igual sirve de red de seguridad ante
            // cualquier desincronización previa entre el set y la lista.
            _handler.Add(interactable);
            return;
        }

        Register(interactable, tracked, _player != null ? _player.transform.position : Center);

        Dispatch();
    }

    private void Untrack(IInteractable interactable, Collider coll)
    {
        if (_handler == null) return;
        if (!_overlaps.TryGetValue(interactable, out var tracked)) return;

        tracked.Colliders.Remove(coll);
        if (tracked.Colliders.Count > 0) return;

        _overlaps.Remove(interactable);
        Unregister(interactable, tracked);

        Dispatch();
    }

    /// <summary>
    /// Alta en la lista de interactuables. El LOS se evalúa en el momento en vez de asumir
    /// visible: si el objeto aparece dentro del radio ya tapado, queda registrado pero bloqueado
    /// y no dispara feedback que el jugador no puede usar.
    /// </summary>
    private void Register(IInteractable interactable, Tracked tracked, Vector3 origin)
    {
        tracked.Registered = true;
        tracked.PendingTime = 0f;
        tracked.Visible = !IsObstructed(origin, interactable, tracked);

        _handler.Add(interactable);
        _handler.SetLineOfSight(interactable, tracked.Visible);

        // La notificación solo en la transición fuera -> dentro: hay interactuables que
        // reaccionan con feedback no idempotente (PlatformTeleport reinicia su ParticleSystem).
        if (tracked.Visible) _transitions.Add((interactable, true));
    }

    private void Unregister(IInteractable interactable, Tracked tracked)
    {
        bool wasVisible = tracked.Visible;

        tracked.Registered = false;
        tracked.Visible = false;
        tracked.PendingTime = 0f;

        _handler.Remove(interactable);

        // Si ya estaba tapado, su feedback está apagado desde que se cruzó la pared: avisar de
        // nuevo dispararía un apagado duplicado.
        if (wasVisible) _transitions.Add((interactable, false));
    }

    /// <summary>
    /// Notifica las transiciones acumuladas. El guard de reentrada es necesario porque un listener
    /// puede llamar a Forget, que a su vez encola y despacha: sin el guard esa llamada anidada
    /// volvería a recorrer las transiciones que el ciclo de afuera todavía no terminó de emitir.
    /// El ciclo relee Count en cada vuelta, así que lo que se encole durante el despacho sale igual.
    /// </summary>
    private void Dispatch()
    {
        if (_dispatching || _transitions.Count == 0) return;

        _dispatching = true;

        for (int i = 0; i < _transitions.Count; i++)
        {
            var (interactable, visible) = _transitions[i];

            if (!IsDestroyed(interactable))
                (interactable as IProximityListener)?.OnPlayerProximity(visible, _player);
        }

        _transitions.Clear();
        _dispatching = false;
    }

    /// <summary>
    /// ¿El interactuable sigue realmente dentro del radio? Se apoya en el AABB de sus colliders
    /// vivos y no en la posición del transform: un glitcheable grande puede tener el pivote lejos
    /// y la superficie al lado del jugador.
    /// </summary>
    private bool IsPresent(Tracked tracked, Vector3 center, out bool hasLiveCollider)
    {
        hasLiveCollider = false;

        tracked.Colliders.RemoveWhere(IsMissing);

        float limit = Radius * RangeTolerance;
        float sqrLimit = limit * limit;

        foreach (var coll in tracked.Colliders)
        {
            if (!coll.enabled || !coll.gameObject.activeInHierarchy) continue;

            hasLiveCollider = true;

            if (coll.bounds.SqrDistance(center) <= sqrLimit) return true;
        }

        return false;
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
