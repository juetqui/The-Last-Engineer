using UnityEngine;

public class PlatformMotor
{
    private readonly Transform _transform;
    private readonly float _baseThreshold;

    public PlatformMotor(Transform transform, IMovablePassenger passenger, float threshold = 0.15f)
    {
        _transform = transform;
        _baseThreshold = threshold;
    }

    public bool InTarget(Vector3 target, float speed = 0f)
    {
        float dynamicThreshold = Mathf.Max(_baseThreshold, speed * Time.deltaTime * 1.1f);
        return Vector3.Distance(_transform.position, target) <= dynamicThreshold;
    }

    public void MoveTowards(Vector3 target, float speed, IMovablePassenger passenger = null)
    {
        Vector3 toTarget = target - _transform.position;
        float distance = toTarget.magnitude;
        float step = speed * Time.deltaTime;

        if (step > distance)
            step = distance;

        Vector3 move = toTarget.normalized * step;
        _transform.position += move;

        if (passenger != null)
            passenger?.OnPlatformMoving(move);
    }

    public void Stop(IMovablePassenger passenger)
    {
        passenger?.OnPlatformMoving(Vector3.zero);
    }
}
