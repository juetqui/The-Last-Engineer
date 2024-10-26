using System.Collections.Generic;
using UnityEngine;

public abstract class TaskManager : MonoBehaviour
{
    [Header("Lists")]
    public List<ConnectionNode> connections;
    [SerializeField] protected List<OpenDoor> _doors;

    [Header("MVC View")]
    [SerializeField] protected AudioSource _source;

    protected int _workingNodes = default, _totalToFinish = default;
    protected bool _running = false;

    protected HashSet<NodeType> _nodesSet = new HashSet<NodeType>();

    public bool Running { get { return _running; } }

    protected abstract void OnAllNodesConnected();
    protected abstract void SetUp();

    protected void OnStart()
    {
        _totalToFinish = connections.Count;
        ValidateAllConnections();
    }

    protected void ValidateAllConnections()
    {
        if (_workingNodes == _totalToFinish && _nodesSet.Count == _totalToFinish)
        {
            _running = true;
            OnAllNodesConnected();
        }
        else _running = false;
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
