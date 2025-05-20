using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEffectController : MonoBehaviour
{
    public static NodeEffectController Instance = null;

    [SerializeField] private NodeType _requiredType;

    public Action<bool> OnToggleObjects = delegate { };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += ToggleObjects;
    }

    private void ToggleObjects(bool toggle, NodeType nodeType)
    {
        if (nodeType != _requiredType) return;

        OnToggleObjects?.Invoke(toggle);
    }
}
