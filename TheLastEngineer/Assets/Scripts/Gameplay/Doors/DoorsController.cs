using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private List<Connection> _connections = new List<Connection>();

    private DoorsView _door;
    private int _activeCount = 0;
    private bool _isOpen = false, _shouldOpen;
    [SerializeField] NodeType tipo = NodeType.Default;
    private void Awake()
    {
        _door = GetComponent<DoorsView>();
        _door.Initialize();
        RecountActiveConnections();
        EvaluateAndApply();
    }
    private void EvaluateAndApply()
    {
        _shouldOpen = (_activeCount == _connections.Count && _connections.Count > 0);
        if (_shouldOpen == _isOpen) return;

        _isOpen = _shouldOpen;
        _door.OpenDoor(_isOpen);
    }
    private void RecountActiveConnections()
    {
        _activeCount = 0;
        foreach (var c in _connections)
        {
            if (c != null && c.IsConnected) _activeCount++;
        }
    }

    private void OnConnectionStateChanged(NodeType type, bool connected)
    {
        if(type == tipo) _activeCount += connected ? 1 : -1;
        //_activeCount += connected ? 1 : -1;
        _activeCount = Mathf.Clamp(_activeCount, 0, _connections.Count);
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
    private void OnEnable() 
    {
        Subscribe(true);
    }
    private void OnDisable()
    {
        Subscribe(false);
    }
}