using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DoorsView _door;
    [SerializeField] private List<Connection> _connections = new List<Connection>();

    // Runtime
    private int _activeCount = 0;
    private bool _isOpen = false, _shouldOpen;

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
        if(type == NodeType.Default) _activeCount += connected ? 1 : -1;
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
        _shouldOpen = (_activeCount == _connections.Count && _connections.Count > 0);

        if (_shouldOpen == _isOpen) return;

        _isOpen = _shouldOpen;
        _door.OpenDoor(_isOpen);
    }
}