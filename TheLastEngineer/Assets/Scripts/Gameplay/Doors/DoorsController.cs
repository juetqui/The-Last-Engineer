using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DoorController : MonoBehaviour
{
    // Se serializa como MonoBehaviour porque Unity no serializa interfaces; OnValidate garantiza que cada
    // elemento implemente IActivationSource. FormerlySerializedAs conserva las conexiones ya asignadas en escenas.
    [FormerlySerializedAs("_connections")]
    [SerializeField] private List<MonoBehaviour> _activationSources = new List<MonoBehaviour>();

    private readonly List<IActivationSource> _sources = new List<IActivationSource>();
    private DoorsView _door;
    private bool _isOpen = false;

    private void Awake()
    {
        _door = GetComponent<DoorsView>();
        _door.Initialize();

        CacheSources();
        Subscribe(true);
    }

    private void Start()
    {
        // Estado inicial: fuentes que ya arrancan activas (p. ej. una Connection con nodo precargado).
        EvaluateAndApply();
    }

    private void OnDestroy()
    {
        Subscribe(false);
    }

    // OnValidate no corre en build y una referencia puede romperse, así que la caché vuelve a filtrar.
    private void CacheSources()
    {
        _sources.Clear();

        foreach (var behaviour in _activationSources)
        {
            if (behaviour is IActivationSource source && !_sources.Contains(source))
                _sources.Add(source);
        }
    }

    private void Subscribe(bool subscribe)
    {
        foreach (var source in _sources)
        {
            if (subscribe)
                source.OnActivationChanged += OnSourceActivationChanged;
            else
                source.OnActivationChanged -= OnSourceActivationChanged;
        }
    }

    // Se recalcula desde IsActive en vez de llevar un contador: no se desfasa si un evento llega antes
    // del Start de la puerta. Cada fuente ya valida su propia condición (nivel requerido, limpieza...).
    private void OnSourceActivationChanged(bool active) => EvaluateAndApply();

    private void EvaluateAndApply()
    {
        bool shouldOpen = _sources.Count > 0 && _sources.TrueForAll(s => s.IsActive);

        if (shouldOpen == _isOpen) return;

        _isOpen = shouldOpen;
        _door.OpenDoor(_isOpen);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < _activationSources.Count; i++)
        {
            var behaviour = _activationSources[i];

            // Null es el hueco que deja el inspector al agrandar la lista: se respeta para poder asignarlo.
            if (behaviour == null || behaviour is IActivationSource) continue;

            // Al arrastrar un GameObject, Unity toma su primer MonoBehaviour, que puede no ser la fuente:
            // se busca la que corresponde en el mismo objeto.
            var source = behaviour.GetComponent<IActivationSource>() as MonoBehaviour;

            if (source == null)
                Debug.LogWarning($"[DoorController] '{behaviour.name}' no implementa IActivationSource y se quitó de la lista.", this);

            _activationSources[i] = source;
        }

        // Una fuente repetida no rompe la lógica (la caché la deduplica), pero confunde en el inspector.
        for (int i = _activationSources.Count - 1; i > 0; i--)
        {
            if (_activationSources[i] != null && _activationSources.IndexOf(_activationSources[i]) < i)
                _activationSources[i] = null;
        }
    }
#endif
}
