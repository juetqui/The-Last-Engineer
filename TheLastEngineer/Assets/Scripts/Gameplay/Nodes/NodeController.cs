using System;
using UnityEngine;

public class NodeController : MonoBehaviour, IInteractable, IProximityListener
{
    #region INTERFACE VARIABLES
    public InteractablePriority Priority => InteractablePriority.High;
    public Color CurrentColor { get { return _currentColor; } }
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    // El nivel ya no vive acá: la fuente de verdad es el GlitchComponent del mismo GameObject.
    // Este controller solo escucha sus cambios y los traduce a color, animación y FX.
    private GlitchComponent _glitch = default;
    public GlitchComponent Glitch => _glitch;
    public GlitchState Level => _glitch.CurrentState;
    public Action<GlitchState> OnUpdatedNodeType = delegate { };

    #region VIEW
    [Header("VIEW")]
    private NodeView _nodeView = default;
    private Collider _collider = default;
    private Renderer _renderer = default;
    private Animator _animator = default;
    private ParticleSystem[] _particles = new ParticleSystem[2];
    private Color _currentColor = default;
    private Outline _outline = default;
    #endregion

    [Header("SHADER")]
    [SerializeField] Shader _desintegrationShader;
    private Vector2 _desintegrationVector = new Vector2(-3, 3);
    private Shader _originalShader;

    private NodeModel _nodeModel = default;
    private Transform _target = default;
    private IConnectable _connectable = default;
    private bool _isChildren = false;

    public Action<bool> OnEnableOutline = delegate { };

    protected void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _outline = GetComponentInChildren<Outline>();
        _particles = GetComponentsInChildren<ParticleSystem>();
        _originalShader = _renderer.material.shader;

        _glitch = GlitchComponent.Ensure(gameObject);
        _glitch.OnGlitchStateChanged += OnLevelChanged;

        _currentColor = ResolveColor(Level);

        _nodeModel = new NodeModel(transform);
        _nodeView = new NodeView(_renderer, _collider, _outline, _currentColor, _animator, _particles);
    }

    protected void Start()
    {
        // El nivel inicial se lee por pull: GlitchComponent no emite el evento en Awake/Start,
        // así que este es el punto donde las views (outline, cristal) reciben el primer valor.
        _nodeView.OnStart();
        _nodeView.UpdateNodeType(Level, _currentColor);
        OnUpdatedNodeType?.Invoke(Level);
    }

    private void OnDestroy()
    {
        if (_glitch != null) _glitch.OnGlitchStateChanged -= OnLevelChanged;
    }

    protected void Update()
    {
        // El estado del collider lo fija Attach() en sus dos ramas; forzarlo por frame acá
        // impedía que cualquier otro sistema lo desactivara temporalmente.
        if (_isChildren) _nodeView.SetCollectedAnim();
    }
    
    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler != null && !playerNodeHandler.HasNode;
    
    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succeeded)
    {
        if (!CanInteract(playerNodeHandler))
        {
            succeeded = false;
            return;
        }

        Vector3 newScale = Vector3.one  * 0.75f;
        Attach(Vector3.zero, playerNodeHandler.AttachTransform, newScale, parentIsPlayer: true);
        succeeded = true;
    }

    public void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false, Quaternion newRot = default)
    {
        if (parentIsPlayer)
        {
            _nodeView.EnableColl(false);
            _nodeView.EnableOutline(false);
            OnEnableOutline?.Invoke(false);
        }
        else
        {
            _nodeView.EnableColl(true);
            _nodeView.EnableOutline(true);
            OnEnableOutline?.Invoke(true);
        }

        if (!parentIsPlayer && newParent != null)
            _connectable = newParent.GetComponent<IConnectable>();
        else if(_connectable != null)
        {
            _connectable.UnsetNode(this);
            _connectable = null;
        }

        if (newParent != null) _isChildren = true;
        else _isChildren = false;

        if (newParent != null && newScale != default)
            _nodeModel.SetPos(newPos, Level, newParent, newScale, newRot);
        else if (newParent != null && newScale == default)
            _nodeModel.SetPos(newPos, Level, newParent);
        else if (newParent == null && newScale == default)
            _nodeModel.SetPos(newPos, Level);

        // Al soltarse, el collider se reactiva DENTRO del trigger del jugador. Que PhysX vuelva
        // a emitir OnTriggerEnter en ese caso es el único eslabón del ciclo soltar -> levantar,
        // y con autoSyncTransforms desactivado no conviene depender de él: nos re-anunciamos.
        if (!parentIsPlayer && PlayerController.Instance != null)
            PlayerController.Instance.RescanInteractable(this, _collider);
    }
    
    // Antes esto era un toggle binario que disparaba el jugador al glitchear. Ahora el nivel lo
    // mueve solo GlitchTransferManager y acá únicamente se reacciona al cambio.
    private void OnLevelChanged(GlitchState level)
    {
        _currentColor = ResolveColor(level);

        _nodeView.UpdateNodeType(level, _currentColor);
        OnUpdatedNodeType?.Invoke(level);
    }

    private Color ResolveColor(GlitchState level) => GlitchPalette.Default.ColorFor(level);

    public void OnPlayerProximity(bool inRange, PlayerController player)
    {
        if (inRange)
        {
            if (_target == null) _target = player.transform;

            _nodeView.SetRangeAnim();
        }
        else
        {
            _target = null;

            _nodeView.SetIdleAnim();
        }
    }

    public void StartDesintegrateShader() => _nodeView.StartDisintegrate(_desintegrationShader, _desintegrationVector);
    public void SetDesintegrateShader(float alpha) => _nodeView.SetDisintegrateAlpha(alpha);
    public void StopDesintegrateShader() => _nodeView.StopDisintegrate(_originalShader);
}
