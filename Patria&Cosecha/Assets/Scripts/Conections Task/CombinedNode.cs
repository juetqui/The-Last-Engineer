using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedNode : ElectricityNode
{
    [SerializeField] private PlayerTDController _player;
    private Renderer _renderer;

    void Start()
    {
        StartUpNode();
    }

    //private void Update()
    //{

    //}

    private void StartUpNode()
    {
        _renderer = GetComponent<Renderer>();

        if (this.NodeType == NodeType.CubeCapsule)
        {
            //_renderer.material.SetColor("CubeColor", FF00FF);
        }
    }
}
