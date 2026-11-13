using UnityEngine;

/// <summary>
/// Test stand-in for PlatformController + PlatformMotor (they live in Assembly-CSharp, which a test asmdef
/// can't reference): same rules, same order and the same Rigidbody.position + transform write.
/// </summary>
[DefaultExecutionOrder(PlatformOrder)]
public class TestPlatformDriver : MonoBehaviour
{
    // Mirrors ExecutionOrder.Platform: before the passenger (0).
    private const int PlatformOrder = -50;

    public Rigidbody Body;
    public Vector3 Target;
    public float Speed;
    public PassengerDisplacement Passenger;
    public bool Moving;

    public bool Arrived { get; private set; }
    public Vector3 TotalDelta { get; private set; }

    private void Update()
    {
        if (!Moving || Arrived) return;

        Vector3 current = transform.position;
        float budget = PlatformStep.StepDistance(Speed, Time.deltaTime, Vector3.Distance(current, Target));
        PlatformStepResult result = PlatformStep.Advance(current, Target, budget);

        Body.position = result.Position;
        transform.position = result.Position;
        TotalDelta += result.Delta;
        Passenger?.Add(result.Delta);
        Arrived = result.Arrived;
    }
}
