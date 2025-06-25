using UnityEngine;

public class DefaultNode : NodeController
{
    private void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    void Update()
    {
        OnUpdate();
    }
}
