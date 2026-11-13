using UnityEngine;

/// <summary>
/// Test stand-in for PlayerModel's vertical movement: same gravity handling and the same two
/// CharacterController.Move calls in Update (platform displacement first, own movement second).
/// </summary>
public class TestPassenger : MonoBehaviour
{
    private const float Gravity = -50f;
    private const float GroundedVerticalSpeed = -1f;

    public CharacterController Controller;
    public readonly PassengerDisplacement Displacement = new PassengerDisplacement();
    public bool Track;

    public int TrackedFrames { get; private set; }
    public int UngroundedFrames { get; private set; }

    private float _verticalSpeed;

    private void Update()
    {
        _verticalSpeed = Controller.isGrounded ? GroundedVerticalSpeed : _verticalSpeed + Gravity * Time.deltaTime;
        Vector3 platformDisplacement = Displacement.Consume();
        if (platformDisplacement != Vector3.zero)
            Controller.Move(platformDisplacement);

        Controller.Move(Vector3.up * (_verticalSpeed * Time.deltaTime));

        if (!Track) return;

        TrackedFrames++;
        if (!Controller.isGrounded) UngroundedFrames++;
    }
}
