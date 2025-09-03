using Cinemachine;
using UnityEngine;

public class ApproachToTarget : MonoBehaviour
{
    private CinemachineFreeLook _camera;
    private float _originalRadius = 0f;

    void Start()
    {
        _camera = GetComponent<CinemachineFreeLook>();
        _originalRadius = _camera.m_Orbits[0].m_Radius;

        CorruptionRemover.Instance.OnCorruptionHit += GetBackToPlace;
        CorruptionRemover.Instance.OnHittingCorruption += GetCloseToTarget;
        CorruptionRemover.Instance.OnCorruptionRemoved += GetBackToPlace;
    }

    private void GetCloseToTarget(float timer)
    {
        timer *= 0.001f;
        _camera.m_Orbits[0].m_Radius -= timer;
    }

    private void GetBackToPlace(Corruption c)
    {
        _camera.m_Orbits[0].m_Radius = _originalRadius;
    }
}
