using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEffectController : MonoBehaviour
{
    public static NodeEffectController Instance = null;

    [SerializeField] private NodeType _requiredType = NodeType.Purple;
    [SerializeField] private float _defaultCD = 1f, _nodeCD = 2.5f;

    private float _currentCD = default;
    private bool _toggle = false;

    public Action<bool> OnToggleObjects = delegate { };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _currentCD = _defaultCD;
    }

    private void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += ToggleObjects;

        StartCoroutine(ToggleObjects());
    }

    private void ToggleObjects(bool hasNode, NodeType nodeType)
    {
        if (nodeType != _requiredType) return;

        if (hasNode)
            _currentCD = _nodeCD;
        else
            _currentCD = _defaultCD;

        //OnToggleObjects?.Invoke(toggle);
    }

    private IEnumerator ToggleObjects()
    {
        yield return new WaitForSeconds(_currentCD);

        OnToggleObjects?.Invoke(_toggle);
        _toggle = !_toggle;

        yield return new WaitForSeconds(_currentCD);

        StartCoroutine(ToggleObjects());
    }
}
