using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionNode : MonoBehaviour
{
    [SerializeField] private ElectricityNode _cubeNode = default, _sphereNode = default, _capsuleNode = default;
    [SerializeField] private NodeType _requiredType = default;
    
    private Material _material;
    private bool _connected = false;
    private NodeType _typeReceived = default;


    private void Start()
    {
        _material = GetComponent<Material>();
        _cubeNode.gameObject.SetActive(false);
        _sphereNode.gameObject.SetActive(false);
        _capsuleNode.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_connected)
        {
            TurnOnReceivedNode();
            CheckReceivedNode();
        }
    }

    private void TurnOnReceivedNode()
    {
        _material.color = new Color(0, 0, 0, 0);
        if (_typeReceived == NodeType.Cube)
        {
            _cubeNode.gameObject.SetActive(true);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(false);
        }
        else if (_typeReceived == NodeType.Sphere)
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(true);
            _capsuleNode.gameObject.SetActive(false);
        }
        else if (_typeReceived == NodeType.Capsule)
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(true);
        }
    }

    private void CheckReceivedNode()
    {
        if (_typeReceived == _requiredType) Debug.Log("CORRECTO");
        else Debug.Log("ERROR");
    }

    public void SetNode(NodeType node)
    {
        _typeReceived = node;
        _connected = true;
    }
}

public enum NodeType
{
    Cube,
    Sphere,
    Capsule
}
