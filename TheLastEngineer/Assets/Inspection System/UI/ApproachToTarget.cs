using Unity.Cinemachine;
using UnityEngine;

public class ApproachToTarget : MonoBehaviour
{
    [SerializeField] private float _timeModifier = 0.00125f;

    private CinemachineOrbitalFollow _orbitalFollow;
    private float _originalRadius = 0f;

    void Start()
    {
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _originalRadius = _orbitalFollow.Orbits.Top.Radius;

        CorruptionRemover.Instance.OnCorruptionHit += GetBackToPlace;
        CorruptionRemover.Instance.OnHittingCorruption += GetCloseToTarget;
        CorruptionRemover.Instance.OnCorruptionRemoved += GetBackToPlace;
    }

    private void GetCloseToTarget(float timer)
    {
        timer *= _timeModifier;
        var orbits = _orbitalFollow.Orbits;
        orbits.Top.Radius -= timer;
        _orbitalFollow.Orbits = orbits;
    }

    private void GetBackToPlace(Corruption c)
    {
        var orbits = _orbitalFollow.Orbits;
        orbits.Top.Radius = _originalRadius;
        _orbitalFollow.Orbits = orbits;
    }
}
