using UnityEngine;

public class SecondaryTM : TaskManager
{
    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
        OnStart();
        SetUp();
    }

    protected override void OnAllNodesConnected()
    {
        _source.Play();
    }

    protected override void SetUp()
    {
        foreach (var connection in connections) connection.SetSecTM(this);

        if (_doors.Count > 0)
        {
            foreach (var door in _doors) door.SetSecTM(this);
        }
    }
}
