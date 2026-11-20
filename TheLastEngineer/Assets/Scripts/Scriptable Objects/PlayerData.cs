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

    [Tooltip("Layers que el CharacterController y el CapsuleCollider excluyen mientras el jugador lleva un nodo en Intangible.")]
    public LayerMask intangibleExcludeLayers = default;

    [Header("Analog Movement")]
    [Tooltip("Inclinación del stick (0..1) → fracción de la velocidad máxima. El teclado siempre entra con 1.")]
    [SerializeField] private AnimationCurve _moveSpeedByTilt = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Tooltip("Fracción mínima de la velocidad apenas el stick sale de la zona muerta.")]
    [SerializeField, Range(0f, 1f)] private float _minMoveSpeedFraction = 0.3f;

    [Tooltip("Suavizado (segundos) del parámetro Blend del Animator (Idle 0 / Walk 0.5 / Run 1).")]
    [SerializeField, Min(0f)] private float _moveAnimBlendDampTime = 0.1f;

    public AnimationCurve MoveSpeedByTilt => _moveSpeedByTilt;
    public float MinMoveSpeedFraction => _minMoveSpeedFraction;
    public float MoveAnimBlendDampTime => _moveAnimBlendDampTime;

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

    [Header("Glitch Transfer")]
    [Tooltip("Rumble de un Set/Take exitoso (x = motor bajo, y = motor alto).")]
    public Vector2 transferInitialRumble = new Vector2(0.35f, 0.5f);

    [Tooltip("Rumble corto y seco cuando la transferencia no se puede hacer.")]
    public Vector2 transferErrorRumble = new Vector2(0f, 0.8f);

    public float transferErrorRumbleDuration = 0.08f;

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
