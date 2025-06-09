using System.Collections.Generic;
using UnityEngine;

public class MultipleListsGlitcheable : Glitcheable
{
    [SerializeField] private List<Transform> _secPosList;
    [SerializeField] private BigPressurePlate _bigPressurePlate = default;

    private void Awake()
    {
        OnAwake();

        if (_bigPressurePlate != null)
        {
            _bigPressurePlate.OnPressed += ChangePosList;
        }
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += CheckNode;
        GlitchActive.Instance.OnStopObject += StopObject;
    }

    void Update()
    {
        if (_canMove && !_isStopped)
        {
            StartCoroutine(StartTimer());
        }
    }

    private void ChangePosList(bool changed)
    {
        if (changed)
        {
            _currentList = _secPosList;
        }
        else
        {
            _currentList = _newPosList;
        }
    }
}
