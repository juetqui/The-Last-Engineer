using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransporterNode : NodeController
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
