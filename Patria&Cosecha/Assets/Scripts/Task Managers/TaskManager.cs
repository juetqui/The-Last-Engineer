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

    protected Dictionary<NodeType, int> _nodesSet = new Dictionary<NodeType, int>();

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
        if (_workingNodes == _totalToFinish)
        {
            _running = true;
            OnAllNodesConnected();
        }
        else _running = false;
    }

    public void AddConnection(NodeType nodeType)
    {
        if (_nodesSet.ContainsKey(nodeType)) _nodesSet[nodeType]++;
        else _nodesSet.Add(nodeType, 1);

        _workingNodes++;
        ValidateAllConnections();
    }

    public void RemoveConnection(NodeType nodeType)
    {
        if (_nodesSet.ContainsKey(nodeType))
        {
            _nodesSet[nodeType]--;

            if (_nodesSet[nodeType] <= 0) _nodesSet.Remove(nodeType);
        }

        _workingNodes--;
        ValidateAllConnections();
    }
}