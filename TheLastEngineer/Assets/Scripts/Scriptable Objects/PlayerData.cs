using UnityEngine;

[CreateAssetMenu]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = default;
    public float upgradedMoveSpeed = default;
    public float rotSpeed = default;
    public float teleportSpeed = default;
    public float fovAngle = default;
    public float coyoteTime = default;
    public float maxWallDist = default;
    public LayerMask wallMask = default;
    public LayerMask glitchDetectionLayer = default;
    public LayerMask defaultLayer = default;
    public LayerMask teleportLayer = default;

    [Header("Dash")]
    public float dashSpeed = default;
    public float dashDuration = default;
    public float dashCD = default;

    [Header("Gamepad Interactions")]
    public float holdInteractionTime = default;

    [Header("Gamepad Rumble")]
    public float lowRumbleFrequency = default;
    public float highRumbleFrequency = default;
    public float rumbleDuration = default;
    public float testForce = default;

    [Header("Audio")]
    public AudioClip walkClip;
    public AudioClip dashClip;
    public AudioClip chargedDashClip;
    public AudioClip liftClip;
    public AudioClip putDownClip;
    public AudioClip emptyHand;
    public AudioClip deathClip;
    public AudioClip fallClip;
}
