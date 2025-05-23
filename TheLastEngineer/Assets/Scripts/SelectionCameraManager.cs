using UnityEngine;
using Cinemachine;

public class SelectionCameraManager : MonoBehaviour
{
    private CinemachineFreeLook _camera;

    private void Awake()
    {
        _camera = GetComponent<CinemachineFreeLook>();
    }

    void Start()
    {
        //RangeWorldPowers.Instance.MaterializeSelection += SetTarget;
    }

    private void SetTarget(Transform target)
    {
        _camera.Follow = target;
    }
}
