using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private ConnectionNode[] _connections;

    private int _workingNodes = default, _totalToFinish = default;
    private bool _running = false;

    public bool Running { get { return _running; } }

    void Start()
    {
        _totalToFinish = _connections.Length;
    }

    void Update()
    {
        Debug.Log("Is running: " + _running);

        if (_workingNodes == _totalToFinish) _running = true;
    }

    public void AddConnection()
    {
        _workingNodes++;
    }
}
