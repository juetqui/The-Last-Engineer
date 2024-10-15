using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private ConnectionNode[] _connections;
    [SerializeField] private GameObject _energyModule, _nodeToConnect;

    private int _workingNodes = default, _totalToFinish = default;
    private bool _running = false;

    private HashSet<NodeType> _nodesSet = new HashSet<NodeType>();

    public bool Running { get { return _running; } }

    void Start()
    {
        _totalToFinish = _connections.Length;
        ValidateAllConnections();
    }

    // Valida si todos los nodos han sido correctamente conectados
    private void ValidateAllConnections()
    {
        if (_workingNodes == _totalToFinish && _nodesSet.Count == _totalToFinish)
        {
            _running = true;
            OnAllNodesConnected();  // Ejecuta lógica cuando todo está conectado
        }
        else
        {
            _running = false;
        }
    }

    // Lógica que ocurre cuando todos los nodos están conectados
    private void OnAllNodesConnected()
    {
        if (_nodeToConnect != null && _energyModule != null)
        {
            Renderer nodeRenderer = _nodeToConnect.GetComponent<Renderer>();
            Renderer energyRenderer = _energyModule.GetComponent<Renderer>();
            if (nodeRenderer != null && energyRenderer != null)
            {
                nodeRenderer.material = energyRenderer.material;
            }
        }
    }

    // Agrega un nodo conectado y valida si se ha completado el estado
    public void AddConnection(NodeType nodeType)
    {
        if (_nodesSet.Add(nodeType))  // Solo suma si el nodo es nuevo
        {
            _workingNodes++;
            ValidateAllConnections();  // Validar cuando se agrega una conexión
        }
    }

    // Remueve un nodo de la lista de conexiones y valida el estado
    public void RemoveConnection(NodeType nodeType)
    {
        if (_nodesSet.Remove(nodeType))  // Solo resta si el nodo estaba presente
        {
            _workingNodes--;
            ValidateAllConnections();  // Validar cuando se elimina una conexión
        }
    }
}
