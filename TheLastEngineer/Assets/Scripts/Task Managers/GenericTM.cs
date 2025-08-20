using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class GenericTM : MonoBehaviour
{
    [Header("Lists")]
    [SerializeField] private List<GenericConnectionController> _connections;
    [SerializeField] private List<NodeType> _requiredTypes;
    [SerializeField] private ParticleSystem _windParticle;
    
    [Header("MVC View")]
    private Animator _animator;
    private AudioSource _source;

    private int _workingNodes = default, _totalToFinish = default;
    protected bool _running = false;

    private Dictionary<NodeType, int> _totalRequired = new Dictionary<NodeType, int>();
    private Dictionary<NodeType, int> _nodesSet = new Dictionary<NodeType, int>();

    public Action<bool> OnRunning;

    public bool Running { get { return _running; } }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _source = GetComponent<AudioSource>();

        SetUp();
    }
    private void OnDestroy()
    {
        _connections.ForEach(c => c.OnNodeConnected -= HandleConnectedNode);
    }

    private void SetUp()
    {
        foreach (var connection in _connections) connection.SetSecTM(this);

        _connections.ForEach(c => c.OnNodeConnected += HandleConnectedNode);
        _totalToFinish = _connections.Count;

        _totalRequired = _requiredTypes
            .GroupBy(type => type)
            .ToDictionary(
                group => group.Key,
                group => group.Count()
            );
    }

    protected virtual void ValidateAllConnections()
    {
        _running = CheckRequirements();

        if (_running && !_source.isPlaying)
        {
            _animator.SetBool("DoorActivated", true);
            _source.Play();
            _windParticle.Play();
            foreach (var item in _connections)
            {
                item.SetPositiveFeedback(true);
            }
        }
        else
        {
            _animator.SetBool("DoorActivated", false);
            _source.Stop();
            _windParticle.Stop();
            foreach (var item in _connections)
            {
                item.SetPositiveFeedback(false);
            }
        }

        OnRunning?.Invoke(_running);
    }

    protected bool CheckRequirements()
    {
        bool hasAllConnections = _totalRequired.All(required =>
        {
            int currentCount = _nodesSet.ContainsKey(required.Key) ? _nodesSet[required.Key] : 0;
            return currentCount == required.Value;
        });

        bool connectionsAreValid = _nodesSet.All(node => _totalRequired.ContainsKey(node.Key));

        return hasAllConnections && connectionsAreValid && _workingNodes == _totalToFinish;
    }

    private void HandleConnectedNode(NodeType node, bool connected)
    {
        if (connected)
        {
            AddConnection(node);
        }
        else
        {
            RemoveConnection(node);
        }
    }

    private bool CanAddNode(NodeType nodeType)
    {
        if (!_totalRequired.ContainsKey(nodeType)) return false;

        int currentCount = _nodesSet.ContainsKey(nodeType) ? _nodesSet[nodeType] : 0;
        int requiredCount = _totalRequired[nodeType];
        
        if (currentCount >= requiredCount) return false;
        if (_workingNodes >= _totalToFinish) return false;

        return true;
    }

    public void AddConnection(NodeType nodeType)
    {
        if (!CanAddNode(nodeType)) return;

        if (_nodesSet.ContainsKey(nodeType)) _nodesSet[nodeType]++;
        else _nodesSet.Add(nodeType, 1);

        _workingNodes++;
        ValidateAllConnections();
    }

    public void RemoveConnection(NodeType nodeType)
    {
        if (!_nodesSet.ContainsKey(nodeType)) return;
            
        _nodesSet[nodeType]--;

        if (_nodesSet[nodeType] <= 0)
            _nodesSet.Remove(nodeType);

        _workingNodes--;
        ValidateAllConnections();
    }
    public void OpenDoor()
    {
        if (!_source.isPlaying)
        {
            _animator.SetBool("DoorActivated", true);
            _source.Play();
        }
    }
    public void CloseDoor()
    {
        _animator.SetBool("DoorActivated", false);

        if (_source.isPlaying)
        {
            _source.Stop();
        }
    }

}
