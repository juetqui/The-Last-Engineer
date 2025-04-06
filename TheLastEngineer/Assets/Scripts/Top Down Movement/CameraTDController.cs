using UnityEngine;

public class CameraTDController : BaseCamera
{
    private void Awake()
    {
        _basePos = transform.position;
    }

    private void Start()
    {
        Adjust();
    }

    private void LateUpdate()
    {
        ApplyBreathEffect();
    }
}
