using Unity.VisualScripting;
using UnityEngine;

public class PlatformMotor
{
    private readonly Transform _transform;
    private readonly float _threshold;

    public PlatformMotor(Transform transform, IMovablePassenger passenger, float threshold = 0.15f)
    {
        _transform = transform;
        _threshold = threshold;
    }

    public bool InTarget(Vector3 target) => Vector3.Distance(_transform.position, target) <= _threshold;

    public void MoveTowards(Vector3 target, float speed, IMovablePassenger passenger = null)
    {
        Vector3 dir = (target - _transform.position).normalized;
        Vector3 displacement = dir * (speed * Time.deltaTime);
        _transform.position += displacement;

        if (passenger != null) passenger?.OnPlatformMoving(displacement);
    }

    public void Stop(IMovablePassenger passenger)
    {
        passenger?.OnPlatformMoving(Vector3.zero);
    }
}
