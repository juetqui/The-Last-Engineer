using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private ConnectionNode[] _connections;
    [SerializeField] private GameObject _door;

    private int _workingNodes = default, _totalToFinish = default;
    private bool _running = false;

    private Dictionary<NodeType, NodeType> _nodesDictionary = new Dictionary<NodeType, NodeType>();
    private NodeType _lastRecieved = default;

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
            _door.SetActive(false);
        }
    }

    public void AddConnection(NodeType nodeType)
    {
        if (!_nodesDictionary.ContainsKey(nodeType))
        {
            _lastRecieved = nodeType;
            _nodesDictionary.Add(nodeType, nodeType);
            _workingNodes++;
        }
        else
        {
            Debug.Log("NODO REPETIDO");
        }
    }
}
