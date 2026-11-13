using UnityEngine;

public class PlatformMotor
{
    private readonly Rigidbody _body;
    private readonly Transform _transform;

    public PlatformMotor(Rigidbody body, Transform transform)
    {
        _body = body;
        _transform = transform;
    }

    /// <summary>Advances one frame toward <paramref name="target"/>. Returns true when it landed exactly on it.</summary>
    public bool Step(Vector3 target, float speed, IMovablePassenger passenger, out float leftover)
    {
        float distance = Vector3.Distance(_transform.position, target);
        return Travel(target, PlatformStep.StepDistance(speed, Time.deltaTime, distance), passenger, out leftover);
    }

    /// <summary>Moves up to <paramref name="distance"/> toward <paramref name="target"/> and carries the passenger along.</summary>
    public bool Travel(Vector3 target, float distance, IMovablePassenger passenger, out float leftover)
    {
        PlatformStepResult result = PlatformStep.Advance(_transform.position, target, distance);
        leftover = result.Leftover;

        if (result.Delta != Vector3.zero)
        {
            // Runs in Update before the player (ExecutionOrder.Platform) on purpose: Rigidbody.position moves
            // the kinematic body in the physics scene right away, so this frame's CharacterController.Move
            // collides with the floor where it is drawn. FixedUpdate + interpolation would leave the physics
            // pose up to a step ahead of the visual one and make the player jitter on vertical platforms.
            _body.position = result.Position;
            _transform.position = result.Position;
            passenger?.OnPlatformMoving(result.Delta);
        }

        return result.Arrived;
    }

    public void Stop(IMovablePassenger passenger)
    {
        passenger?.OnPlatformMoving(Vector3.zero);
    }
}
