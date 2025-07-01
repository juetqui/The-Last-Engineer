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
        GlitchActive.Instance.OnStopObject += StopObject;
        _timerController.OnTimerCycleComplete += UpdateTarget;
        _timerController.OnPhaseChanged += CheckTimerPhase;
    }

    private void OnCycleStart()
    {
        if (_isStopped || !_isCorrupted) return;
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
