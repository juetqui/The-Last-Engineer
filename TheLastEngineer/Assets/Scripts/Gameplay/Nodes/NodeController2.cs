using System;
using UnityEngine;

[RequireComponent(typeof(NodeViewMono))]
public class NodeController2 : MonoBehaviour, IInteractable
{
    [Header("Config (Model)")]
    [SerializeField] private NodeType _initialType = NodeType.Default;
    [SerializeField] private bool _isChildAtStart = false;
    private float _minY = -0.5f, _maxY = 0.5f, _moveSpeed = 5f, _rotSpeed = 5f;

    [Header("Colors (by type)")]
    [SerializeField] private Color _defaultOutline = Color.cyan;
    [SerializeField] private Color _corruptionOutline = Color.magenta;

    [Header("Env/Attach")]
    [SerializeField] private LayerMask _floorLayer;
    [SerializeField] private bool _autoFloatIfNotChild = true;

    [Header("Disintegrate")]
    [SerializeField] private Shader _disintegrateShader;
    [SerializeField] private Vector2 _disintegrateMinMax = new(-3, 3);
    [SerializeField] private Color _disintegrateColor = Color.white;

    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => true;
    private bool IsAttached => transform.parent != null;

    public event Action<NodeType> OnUpdatedNodeType = delegate { };

    // Runtime
    private INodeView _view;
    private NodeModel2 _model;
    private Shader _originalShader;
    private Transform _feedbackLookAt; // si lo necesitás, asignalo por inspector
    private Transform _targetInRange;

    // Dependencias externas (opcionalmente cacheadas)
    private IConnectable _connectable;

    private void Awake()
    {
        _view = GetComponent<INodeView>();
        var startPos = transform.position;
        _model = new NodeModel2(_initialType, _minY, _maxY, _moveSpeed, _rotSpeed, startPos);
        _model.SetAsChild(_isChildAtStart);

        // Inicializa View
        _view.Initialize(GetOutlineFor(_model.NodeType));

        // Guardamos el shader original
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer) _originalShader = renderer.material.shader;
    }

    private void Start()
    {
        UpdateTypeVisuals(); // sincroniza outline/emit con el tipo inicial
    }

    private void Update()
    {
        if (!_model.IsChild && _autoFloatIfNotChild)
        {
            // Aplica Y oscilatoria calculada por el model
            var pos = transform.position;
            pos.y = _model.EvaluateY(Time.time);
            transform.position = pos;

            // Chequeo suelo (como antes), pero sin acoplarlo al Model
            if (!Physics.Raycast(transform.position, Vector3.down, out _, 3f, _floorLayer))
                transform.position -= Vector3.up * Time.deltaTime * 15f;
        }

        // FX de proximidad (sólo cuando el target está y el tipo es Corrupted)
        bool playFX = _targetInRange && _model.NodeType == NodeType.Corrupted;
        _view.PlayProximityFX(playFX);
    }

    // ==== Interacción pública =====
    public bool CanInteract(PlayerTDController player) => !player.HasNode();

    public void Interact(PlayerTDController player, out bool succeeded)
    {
        if (!CanInteract(player))
        {
            succeeded = false;
            return;
        }

        // Adjuntamos al player
        Attach(player.transform, player.attachPos, new Vector3(0.6f, 0.6f, 0.6f), parentIsPlayer: true);
        succeeded = true;
    }

    // (Overload opcional compatible con tu firma)
    public void Attach(Transform newParent, Vector3 localPos, Vector3 newScale, bool parentIsPlayer)
    {
        if (parentIsPlayer)
        {
            GlitchActive.Instance.OnChangeObjectState += OnChangeObjectState;
            _view.EnableCollider(false);
        }
        else
        {
            GlitchActive.Instance.OnChangeObjectState -= OnChangeObjectState;
            _view.EnableCollider(true);
        }

        // Cachear connectable si el parent lo es
        _connectable = parentIsPlayer ? null : newParent.GetComponent<IConnectable>();
        _model.SetAsChild(newParent != null);

        // Aplicar transformaciones (esta parte SI es Controller)
        transform.SetParent(newParent, false);
        transform.localPosition = localPos;
        transform.localRotation = newParent ? Quaternion.LookRotation(newParent.forward) : Quaternion.identity;
        transform.localScale = newScale == default ? Vector3.one : newScale;

        _model.UpdateInitialFrom(transform.position);

        if (_model.IsChild && _connectable != null)
        {
            // Si estaba dentro de una conexión previa, liberarla
            _connectable.UnsetNode(this);
            _connectable = null;
        }
    }

    public void StartDisintegrate() => _view.StartDisintegrate(_disintegrateShader, _disintegrateColor, _disintegrateMinMax);
    public void SetDisintegrateAlpha(float a) => _view.SetDisintegrateAlpha(a);
    public void StopDisintegrate() => _view.StopDisintegrate(_originalShader);

    // ==== Triggers de proximidad ====
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerTDController>(out var player))
        {
            _targetInRange = player.transform;
            _view.SetInRange();
        }
        else if (other.CompareTag("Void"))
        {
            transform.position = _model.ResetWorldPos;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerTDController>(out _))
        {
            _targetInRange = null;
            _view.SetIdle();
        }
    }

    private void OnDisable()
    {
        if (GlitchActive.Instance != null)
            GlitchActive.Instance.OnChangeObjectState -= OnChangeObjectState;
    }

    // ==== Corrupción / Toggle ====
    private void OnChangeObjectState(Glitcheable gl, InteractionOutcome outcome)
    {
        if (gl == null) return;

        bool newState = _model.NodeType != NodeType.Default; // true si Corrupted
        if (gl.ChangeCorruptionState(_model.NodeType, newState))
        {
            ToggleNodeType();
        }
    }

    private void ToggleNodeType()
    {
        _model.ToggleType();
        UpdateTypeVisuals();
        OnUpdatedNodeType?.Invoke(_model.NodeType);
    }

    private void UpdateTypeVisuals()
    {
        var c = GetOutlineFor(_model.NodeType);
        _view.SetOutlineColor(c);
    }

    private Color GetOutlineFor(NodeType t) => t == NodeType.Default ? _defaultOutline : _corruptionOutline;

    // ===== Implementación de IInteractable genérica (si la necesitás) =====
    public void Interact(GameObject interactor) { /* opcional */ }
    public bool CanInteract(GameObject interactor) => true;
}
