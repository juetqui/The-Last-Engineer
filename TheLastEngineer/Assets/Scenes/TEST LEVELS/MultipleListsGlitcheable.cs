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
        _timerController.OnTimerCycleStart += OnCycleStart;
        _timerController.OnTimerCycleComplete += UpdateTarget;
    }

    void Update()
    {
        UpdateTimer();
    }

    private void OnCycleStart()
    {
        if (_isStopped) return;

        _timer.fillAmount = 1f;
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
