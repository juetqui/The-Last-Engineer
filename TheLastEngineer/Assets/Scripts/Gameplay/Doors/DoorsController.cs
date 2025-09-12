using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DoorsView _door;
    [SerializeField] private List<Connection> _connections = new List<Connection>();

    [Header("Condición de apertura")]
    [Tooltip("Si está activo, se requiere que TODAS las conexiones estén activas.")]
    [SerializeField] private bool _requireAllConnections = true;

    [Tooltip("Si requireAllConnections = false, se abrirá cuando la cantidad de conexiones activas alcance este umbral.")]
    [SerializeField] private int _requiredConnectionsCount = 1;

    // Runtime
    private int _activeCount = 0;
    private bool _isOpen = false;

    private void OnEnable()
    {
        Subscribe(true);
    }

    private void OnDisable()
    {
        Subscribe(false);
    }

    private void Start()
    {
        // IMPORTANTE: Puede haber condiciones de orden de ejecución con Connection.Start().
        // Para estar seguros de leer el estado real, inicializamos al final del primer frame.
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null; // espera 1 frame
        RecountActiveConnections();
        EvaluateAndApply();
    }

    private void Subscribe(bool subscribe)
    {
        if (_connections == null) return;

        foreach (var c in _connections)
        {
            if (c == null) continue;

            if (subscribe)
                c.OnNodeConnected += OnConnectionStateChanged;
            else
                c.OnNodeConnected -= OnConnectionStateChanged;
        }
    }

    private void OnConnectionStateChanged(NodeType type, bool connected)
    {
        _activeCount += connected ? 1 : -1;
        _activeCount = Mathf.Clamp(_activeCount, 0, _connections.Count);
        EvaluateAndApply();
    }

    private void RecountActiveConnections()
    {
        _activeCount = 0;
        foreach (var c in _connections)
        {
            if (c != null && c.IsConnected) _activeCount++;
        }
    }

    private void EvaluateAndApply()
    {
        bool shouldOpen;

        if (_requireAllConnections)
        {
            shouldOpen = (_activeCount == _connections.Count && _connections.Count > 0);
        }
        else
        {
            var threshold = Mathf.Clamp(_requiredConnectionsCount, 1, Mathf.Max(1, _connections.Count));
            shouldOpen = (_activeCount >= threshold);
        }

        if (shouldOpen == _isOpen) return; // nada que cambiar

        _isOpen = shouldOpen;
        _door.OpenDoor(_isOpen);
    }
}