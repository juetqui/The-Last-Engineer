using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SelectionCameraMAnager : MonoBehaviour
{
    private CinemachineFreeLook _camera;

    private void Awake()
    {
        _camera = GetComponent<CinemachineFreeLook>();
    }

    void Start()
    {
        RangeWorldPowers.Instance.MaterializeSelection += SetTarget;
    }

    private void SetTarget(Transform target)
    {
        _camera.Follow = target;
    }
}
