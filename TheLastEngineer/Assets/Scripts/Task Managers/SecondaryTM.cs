using UnityEngine;

public class SecondaryTM : TaskManager
{
    private Animator _animator = default;

    private void Awake()
    {
        OnAwake();
        _animator = GetComponent<Animator>();

        onRunning += OpenDoor;
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
    }

    private void OpenDoor(bool isRunning)
    {
        _animator.SetBool("Open", isRunning);
    }
}
