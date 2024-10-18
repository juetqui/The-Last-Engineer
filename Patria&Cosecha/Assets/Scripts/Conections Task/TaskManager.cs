using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private ConnectionNode[] _connections;
    [SerializeField] private GameObject _energyModule, _nodeToConnect;
    [SerializeField] private AudioSource _source;

    private int _workingNodes = default, _totalToFinish = default;
    private bool _running = false;

    private HashSet<NodeType> _nodesSet = new HashSet<NodeType>();

    public bool Running { get { return _running; } }

    void Start()
    {
        _totalToFinish = _connections.Length;
        ValidateAllConnections();
    }

    private void ValidateAllConnections()
    {
        if (_workingNodes == _totalToFinish && _nodesSet.Count == _totalToFinish)
        {
            _running = true;
            OnAllNodesConnected();
        }
        else
        {
            _running = false;
        }
    }

    private void OnAllNodesConnected()
    {
        if (_nodeToConnect != null && _energyModule != null)
        {
            Renderer nodeRenderer = _nodeToConnect.GetComponent<Renderer>();
            Renderer energyRenderer = _energyModule.GetComponent<Renderer>();
            if (nodeRenderer != null && energyRenderer != null)
            {
                nodeRenderer.material = energyRenderer.material;
                _source.Play();
            }
        }
    }

    public void AddConnection(NodeType nodeType)
    {
        if (_nodesSet.Add(nodeType))
        {
            _workingNodes++;
            ValidateAllConnections();
        }
    }

    public void RemoveConnection(NodeType nodeType)
    {
        if (_nodesSet.Remove(nodeType))
        {
            _workingNodes--;
            ValidateAllConnections();
        }
    }
}
