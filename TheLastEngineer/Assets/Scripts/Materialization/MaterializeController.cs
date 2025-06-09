using System;
using System.Collections;
using UnityEngine;

public class MaterializeController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredType;
    [SerializeField] private float _defaultCD = 0.5f;
    //[SerializeField] private float _nodeCD = 2.5f;

    public static MaterializeController Instance = null;
    private PlayerTDController _player = null;
    private float _currentCD = default;
    private bool _materialize = false;
    
    public Action<bool> OnMaterialize;

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
        _player = PlayerTDController.Instance;
        _player.OnNodeGrabed += Materialize;

        //StartCoroutine(ToggleMaterialization());
    }

    private void Materialize(bool hasNode, NodeType nodeType)
    {
        if (nodeType != _requiredType) return;

        //if (hasNode)
        //    _currentCD = _nodeCD;
        //else
        //    _currentCD = _defaultCD;

        OnMaterialize?.Invoke(hasNode);
    }

    private IEnumerator ToggleMaterialization()
    {
        yield return new WaitForSeconds(_currentCD);

        OnMaterialize(_materialize);
        _materialize = !_materialize;

        yield return new WaitForSeconds(_currentCD);

        StartCoroutine(ToggleMaterialization());
    }
}
