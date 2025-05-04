using System;
using UnityEngine;

public class MaterializeController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredType;

    public static MaterializeController Instance = null;
    private PlayerTDController _player = null;
    
    public Action<bool> OnMaterialize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        OnMaterialize?.Invoke(false);
    }

    private void Start()
    {
        _player = PlayerTDController.Instance;
        _player.OnNodeGrabed += Materialize;
    }

    private void Materialize(bool materialize, NodeType nodeType)
    {
        if (nodeType != _requiredType) return;
        OnMaterialize?.Invoke(!materialize);
    }
}
