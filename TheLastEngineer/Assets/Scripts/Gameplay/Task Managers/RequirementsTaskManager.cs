using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TaskManager;
using static Unity.VisualScripting.Member;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// Valida conexiones por cantidad simple o por tipo requerido.
/// Expone un único evento RunningChanged y métodos idempotentes de feedback.
/// </summary>
public class RequirementsTaskManager : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private List<GenericConnectionController> _connections = new();
    [Tooltip("Modo simple: ignora tipos y valida que todas las conexiones estén activas")]
    [SerializeField] private bool _simpleCountMode = false;
    [SerializeField] private List<NodeType> _requiredTypes = new();

    [Header("View/FX")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _source;
    [SerializeField] private ParticleSystem _windParticle;

    // Runtime
    private readonly Dictionary<NodeType, int> _required = new();
    private readonly Dictionary<NodeType, int> _current = new();
    private int _working;
    private int _total;
    private bool _running;

    public event Action<bool> RunningChanged = delegate { };
    public bool IsRunning => _running;

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_source == null) _source = GetComponent<AudioSource>();

        foreach (var c in _connections)
        {
            //c.SetSecTM(this); // compat: permite que las conexiones te notifiquen
            c.OnNodeConnected += OnNodeConnected;
        }

        _total = _connections.Count;

        if (!_simpleCountMode)
        {
            foreach (var g in _requiredTypes.GroupBy(t => t))
                _required[g.Key] = g.Count();
        }
        Validate();
    }

    private void OnDestroy()
    {
        foreach (var c in _connections)
            c.OnNodeConnected -= OnNodeConnected;
    }

    private void OnNodeConnected(NodeType type, bool active)
    {
        if (_simpleCountMode)
        {
            _working += active ? 1 : -1;
        }
        else
        {
            if (!_required.ContainsKey(type)) { Validate(); return; }
            int cur = _current.ContainsKey(type) ? _current[type] : 0;
            cur = Mathf.Clamp(cur + (active ? 1 : -1), 0, _required[type]);
        }
    }
    private void Validate()
    {
        bool ok = _simpleCountMode
        ? _working == _total
        : _required.All(req => _current.TryGetValue(req.Key, out var count) && count == req.Value) && _working == _total;

        SetRunning(ok);
    }

    private void SetRunning(bool value)
    {
        if (_running == value) return;
        _running = value;
        RunningChanged.Invoke(_running);
        if (_animator) _animator.SetBool("DoorActivated", _running);
        if (_windParticle) { if (_running && !_windParticle.isPlaying) _windParticle.Play(); else if (!_running && _windParticle.isPlaying) _windParticle.Stop(); }
        if (_source) { if (_running && !_source.isPlaying) _source.Play(); else if (!_running && _source.isPlaying) _source.Stop(); }
    }
}