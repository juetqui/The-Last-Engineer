using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Header("Connections")]
    [Tooltip("Arrastrá aquí TODOS los GenericConnectionController de la escena")]
    [SerializeField] private List<Connection> _genericConnections = new();

    [Tooltip("Si es true, solo valida que TODAS las conexiones estén activas, sin importar los tipos.")]
    [SerializeField] private bool _simpleCountMode = true;

    [Tooltip("Si el modo no es simple, estos son los tipos requeridos y su cantidad (puede repetir tipos).")]
    [SerializeField] private List<NodeType> _requiredTypes = new();

    [Header("Feedback / FX opcional")]
    [SerializeField] private Animator _animator;             // bool "DoorActivated"
    [SerializeField] private AudioSource _source;            // loop/one-shot cuando corre
    [SerializeField] private ParticleSystem _particle;       // partículas al correr
    [SerializeField] private Light _light;                   // luz que sube al correr
    [SerializeField] private float _lightTarget = 30f;
    [SerializeField] private float _lightRiseSpeed = 5f;
    [SerializeField] private AudioSource _winAS;             // audio de “win” opcional

    // --- Runtime ---
    private readonly Dictionary<NodeType, int> _required = new();
    private readonly Dictionary<NodeType, int> _current = new();

    private int _working;      // conexiones activas actuales (todas)
    private int _total;        // total de conexiones que deben estar activas
    private bool _running;

    public event Action<bool> RunningChanged = delegate { };
    public bool IsRunning => _running;

    private void Awake()
    {
        // Autoreferencias seguras
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_source == null) _source = GetComponent<AudioSource>();

        // Suscribirse a TODOS los controladores disponibles
        foreach (var c in _genericConnections.Where(c => c != null))
            c.OnNodeConnected += OnNodeChanged;

        _total = _genericConnections.Count ;

        // Si NO es simple, armamos requeridos por tipo (Tipo -> Cantidad)
        if (!_simpleCountMode)
        {
            foreach (var g in _requiredTypes.GroupBy(t => t))
                _required[g.Key] = g.Count();
        }
        Validate();
    }

    private void OnDestroy()
    {
        foreach (var c in _genericConnections.Where(c => c != null))
            c.OnNodeConnected -= OnNodeChanged;
    }

    private void Update()
    {
        // Efecto de luz opcional
        if (_running && _light != null && _light.intensity < _lightTarget)
            _light.intensity = Mathf.Min(_lightTarget, _light.intensity + _lightRiseSpeed * Time.deltaTime);
    }

    private void OnNodeChanged(NodeType type, bool active)
    {
        // Conteo global de conexiones activas
        _working += active ? 1 : -1;
        _working = Mathf.Max(0, _working);

        if (!_simpleCountMode)
        {
            // Validación por tipos
            if (!_required.ContainsKey(type))
            {
                Validate();
                return;
            }

            int cur = _current.ContainsKey(type) ? _current[type] : 0;
            cur = Mathf.Clamp(cur + (active ? 1 : -1), 0, _required[type]);
            _current[type] = cur;
        }

        Validate();
    }

    private void Validate()
    {
        bool ok;

        if (_simpleCountMode)
        {
            ok = (_working == _total);
        }
        else
        {
            bool allTypesOk = _required.All(req =>
                _current.TryGetValue(req.Key, out var count) && count == req.Value);

            ok = allTypesOk && (_working == _total);
        }

        SetRunning(ok);
    }

    private void SetRunning(bool value)
    {
        if (_running == value) return;
        _running = value;

        // Evento público para VFX externos (DoorLights, ParticlesColorChanger, etc.)
        RunningChanged.Invoke(_running);

        // Feedback local (opcional)
        if (_animator) _animator.SetBool("DoorActivated", _running);

        if (_particle)
        {
            if (_running && !_particle.isPlaying) _particle.Play();
            else if (!_running && _particle.isPlaying) _particle.Stop();
        }

        if (_source)
        {
            if (_running && !_source.isPlaying) _source.Play();
            else if (!_running && _source.isPlaying) _source.Stop();
        }

        if (_winAS && _running) _winAS.Play();

        // Reset de luz si se cerró
        if (!_running && _light) _light.intensity = 0f;
    }
}
