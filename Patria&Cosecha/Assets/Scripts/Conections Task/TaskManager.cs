using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private ConnectionNode[] _connections;
    [SerializeField] private GameObject _energyModule, _nodeToConnect;

    private int _workingNodes = default, _totalToFinish = default;
    private bool _running = false;

    private Dictionary<NodeType, NodeType> _nodesDictionary = new Dictionary<NodeType, NodeType>();

    public bool Running { get { return _running; } }

    void Start()
    {
        _totalToFinish = _connections.Length;
    }

    void Update()
    {
        if (_workingNodes == _totalToFinish && _nodesDictionary.Count == _totalToFinish)
        {
            _running = true;
            _nodeToConnect.GetComponent<Renderer>().material = _energyModule.GetComponent<Renderer>().material;
        }
    }

    public void AddConnection(NodeType nodeType)
    {
        if (!_nodesDictionary.ContainsKey(nodeType))
        {
            _nodesDictionary.Add(nodeType, nodeType);
            _workingNodes++;
        }
    }

    public void RemoveConnection(NodeType nodeType)
    {
        if (_nodesDictionary.ContainsKey(nodeType))
        {
            _nodesDictionary.Remove(nodeType);
            _workingNodes--;
        }
    }
}
