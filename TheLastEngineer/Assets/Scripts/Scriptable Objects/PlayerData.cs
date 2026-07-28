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

    [Header("Interaction")]
    public float interactionRadius = 2.5f;

    [Tooltip("Cada cuánto se recalcula la línea de visión de los interactuables en rango. 0 = todos los frames.")]
    public float losCheckInterval = 0f;

    [Tooltip("Tiempo que un cambio de visibilidad tiene que sostenerse antes de aplicarse. Evita el parpadeo al rozar columnas.")]
    public float losDebounceTime = 0.08f;

    [Tooltip("Altura a la que viaja el rayo de línea de visión, medida desde el pivote del jugador y del objetivo.")]
    public float losHeightOffset = 2f;

    [Tooltip("Cuánto se acorta el rayo antes del objetivo, para no impactar la superficie sobre la que está montado.")]
    public float losEndMargin = 0.25f;

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
