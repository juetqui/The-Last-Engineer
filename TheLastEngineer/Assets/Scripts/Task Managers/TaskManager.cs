using System.Collections.Generic;
using UnityEngine;

public abstract class TaskManager : MonoBehaviour
{
    [Header("Lists")]
    [SerializeField] protected List<SpecificConnectionController> connections;
    //[SerializeField] protected ElectricityController _elecController;

    [Header("MVC View")]
    [SerializeField] protected AudioSource _source;

    protected int _workingNodes = default, _totalToFinish = default, _totalForDictionary = default;
    protected bool _running = false;
    
    public delegate void OnRunning(bool isRunning);
    public event OnRunning onRunning = default;

    protected Dictionary<NodeType, int> _nodesSet = new Dictionary<NodeType, int>();

    public bool Running { get { return _running; } }

    protected abstract void OnAllNodesConnected();
    protected abstract void SetUp();

    protected void OnAwake()
    {
        _totalForDictionary = _nodesSet.Count;
    }

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
        else
            _running = false;

        onRunning?.Invoke(_running);
    }

    public void AddConnection(NodeType nodeType)
    {
        if (_nodesSet.ContainsKey(nodeType)) _nodesSet[nodeType]++;
        else _nodesSet.Add(nodeType, 1);

        //if (_elecController != null)
        //    _elecController.MoveSpline();

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
