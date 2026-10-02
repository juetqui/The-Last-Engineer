using UnityEngine;
using PrimeTween;

/// <summary>
/// Posiciona un RectTransform de UI sobre el IInteractable que el jugador tiene detectado, con un
/// margen en Y en pixeles de pantalla. Sirve solo (asignandole el visual que se prende y apaga con
/// el target) o como base de HUDs que necesiten mas comportamiento, como UpdateCrosshair.
/// </summary>
public class UpdatePosToTarget : MonoBehaviour
{
    [Tooltip("Que señal del PlayerController sigue: el interactuable de Interact o el glitcheable de Set/Take.")]
    [SerializeField] private UITargetSource _source = UITargetSource.Interactable;

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
    [SerializeField] private float _xOffset = 0f;

    [Header("Visual Scale Tween")]
    [SerializeField] private float _minTargetScale = 0f;
    [SerializeField] private float _maxTargetScale = 1f;
    [SerializeField] private float _scaleDuration = 0.2f;
    [SerializeField] private float _scaleDelay = 0.25f;
    [SerializeField] private Ease _scaleEase = Ease.OutBack;

    protected RectTransform Rect { get; private set; }
    protected IInteractable CurrentTarget { get; private set; }

    private RectTransform _visualRect;
    private bool _visualShown;
    private UIScaleTween _scaleTween;

    /// <summary>Los hijos que solo tienen sentido con una de las dos señales la fijan acá.</summary>
    protected virtual UITargetSource Source => _source;

    protected virtual void Awake()
    {
        if (_camera == null) _camera = Camera.main;

        _scaleTween = new UIScaleTween(_minTargetScale, _maxTargetScale, _scaleDuration, _scaleEase);

        Rect = ResolveRectToMove();

        if (_visual != null)
        {
            _visualRect = _visual.transform as RectTransform;

            // Arrancamos oculto y en la escala chica asi el primer show crece desde cero. La
            // excepcion es el visual mal cableado a este mismo objeto: apagarlo en el Awake nos
            // llevaria puesto nuestro Start y el de los componentes hermanos, que quedarian sin
            // suscribirse para siempre. En ese caso respetamos el estado que venga del prefab.
            _visualShown = _visual == gameObject && _visual.activeSelf;

            if (!_visualShown) _visual.SetActive(false);

            // if (_visualRect != null)
            //     _visualRect.localScale = Vector3.one * (_visualShown ? _maxTargetScale : _minTargetScale);
        }
    }

    protected virtual void Start()
    {
        Subscribe(true);
        SetVisual(false);
    }

    protected virtual void OnDestroy()
    {
        _scaleTween?.Stop(_visualRect);

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
        var player = PlayerController.Instance;
        if (player == null) return;

        if (Source == UITargetSource.Glitcheable)
        {
            if (value) player.OnGlitcheableDetected += SetGlitcheableTarget;
            else player.OnGlitcheableDetected -= SetGlitcheableTarget;
        }
        else
        {
            if (value) player.OnInteractableDetected += SetTarget;
            else player.OnInteractableDetected -= SetTarget;
        }
    }

    // El == de UnityEngine.Object descarta un glitcheable destruido antes de perder el tipo concreto.
    private void SetGlitcheableTarget(Glitcheable glitcheable) => SetTarget(glitcheable != null ? glitcheable : null);

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

        screenPosition.x += _xOffset;
        screenPosition.y += _yOffset;
        Rect.position = screenPosition;

        OnPositionUpdated(screenPosition);
    }

    #region -----HOOKS-----
    protected virtual void OnTargetChanged(IInteractable target) => SetVisual(target != null);

    protected virtual void OnPositionUpdated(Vector3 screenPosition) { }

    protected virtual void OnTargetUnavailable() => SetVisual(false);

    /// <summary>
    /// Se llama recien cuando el visual termino de achicarse y ya quedo desactivado, no apenas se
    /// pide ocultarlo. Los hijos lo usan para limpiar estado sin cortar la transicion.
    /// </summary>
    protected virtual void OnVisualHidden() { }
    #endregion

    // El guard va contra _visualShown y no contra activeSelf: mientras el visual se esta achicando
    // sigue activo, asi que activeSelf mentiria y un pedido de mostrar volveria temprano.
    protected void SetVisual(bool value)
    {
        if (_visual == null || _visualRect == null || _visualShown == value) return;

        _visualShown = value;
        SetScale(value);
    }

    /// <summary>
    /// Escala el visual entre _minTargetScale y _maxTargetScale para que no aparezca ni desaparezca
    /// de golpe. Al mostrar activa primero y despues crece; al ocultar achica y recien ahi desactiva.
    /// </summary>
    protected void SetScale(bool value)
    {
        if (_visual == null || _visualRect == null) return;

        if (value)
        {
            _visual.SetActive(true);
            _scaleTween.Show(_visualRect);
        }
        else
        {
            _scaleTween.Hide(_visualRect, () =>
            {
                _visual.SetActive(false);
                OnVisualHidden();
            });
        }
    }
}
