using UnityEngine;

/// <summary>
/// Posiciona un RectTransform de UI sobre el IInteractable que el jugador tiene detectado, con un
/// margen en Y en pixeles de pantalla. Sirve solo (asignandole el visual que se prende y apaga con
/// el target) o como base de HUDs que necesiten mas comportamiento, como UpdateCrosshair.
/// </summary>
public class UpdatePosToTarget : MonoBehaviour
{
    #region -----REFS-----
    [Header("Refs")]
    [SerializeField] protected Camera _camera;
    [Tooltip("Si queda vacio se usa el RectTransform de este mismo objeto.")]
    [SerializeField] private RectTransform _rectToMove;
    [Tooltip("Opcional: se prende y apaga junto con el target. Nunca este mismo objeto.")]
    [SerializeField] private GameObject _visual;
    #endregion

    [Header("Screen Offset")]
    [SerializeField] private float _yOffset = 0f;

    protected RectTransform Rect { get; private set; }
    protected IInteractable CurrentTarget { get; private set; }

    protected virtual void Awake()
    {
        if (_camera == null) _camera = Camera.main;

        Rect = ResolveRectToMove();
    }

    protected virtual void Start()
    {
        Subscribe(true);
    }

    protected virtual void OnDestroy()
    {
        Subscribe(false);
    }

    /// <summary>
    /// Que RectTransform mueve este script. Los hijos lo overridean cuando el grafico no vive en el
    /// mismo objeto que el componente.
    /// </summary>
    protected virtual RectTransform ResolveRectToMove()
        => _rectToMove != null ? _rectToMove : GetComponent<RectTransform>();

    private void Subscribe(bool value)
    {
        if (PlayerController.Instance == null) return;

        if (value) PlayerController.Instance.OnInteractableDetected += SetTarget;
        else PlayerController.Instance.OnInteractableDetected -= SetTarget;
    }

    // OnInteractableDetected se dispara todos los frames: el guard de cambio hace que los hijos
    // reciban OnTargetChanged solo cuando el objetivo realmente cambia.
    private void SetTarget(IInteractable interactable)
    {
        // El fake-null de Unity no aplica sobre una referencia de interfaz, asi que chequeamos el
        // Transform para descartar un interactuable ya destruido.
        var target = interactable == null || interactable.Transform == null ? null : interactable;

        if (ReferenceEquals(target, CurrentTarget)) return;

        CurrentTarget = target;
        OnTargetChanged(target);

        if (target == null) OnTargetUnavailable();
    }

    protected virtual void Update()
    {
        if (Rect == null || CurrentTarget == null || CurrentTarget.Transform == null) return;

        var screenPosition = _camera.WorldToScreenPoint(CurrentTarget.Transform.position);

        if (screenPosition.z <= 0)
        {
            OnTargetUnavailable();
            return;
        }

        screenPosition.y += _yOffset;
        Rect.position = screenPosition;

        OnPositionUpdated(screenPosition);
    }

    #region -----HOOKS-----
    protected virtual void OnTargetChanged(IInteractable target) => SetVisual(target != null);

    protected virtual void OnPositionUpdated(Vector3 screenPosition) { }

    protected virtual void OnTargetUnavailable() => SetVisual(false);
    #endregion

    private void SetVisual(bool value)
    {
        if (_visual == null || _visual.activeSelf == value) return;

        _visual.SetActive(value);
    }
}
