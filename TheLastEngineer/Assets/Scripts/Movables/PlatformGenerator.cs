using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [SerializeField] private List<NodeType> _correctTypes = new List<NodeType>();
    [SerializeField] private GenericConnectionController _connection = default;
    [SerializeField] private Material _ActiveMat;
    [SerializeField] private Material _DisabledMat;

    private MeshRenderer _renderer = default;
    private BoxCollider _collider = default;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<BoxCollider>();
        
        _collider.enabled = false;

        _connection.OnNodeConnected += GeneratePlatform;
    }

    private void GeneratePlatform(NodeType node, bool isConnected)
    {
        foreach (var correctType in _correctTypes)
        {
            if (node == correctType && isConnected)
            {
                _renderer.material = _ActiveMat;
                _collider.enabled = true;
                break;
            }
            else
            {
                _renderer.material = _DisabledMat;
                _collider.enabled = false;
            }
        }
    }
}
