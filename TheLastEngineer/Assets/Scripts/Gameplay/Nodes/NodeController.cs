using System;
using UnityEngine;

public class NodeController : MonoBehaviour, IInteractable
{
    #region MODEL
    [Header("Model")]
    [SerializeField] private Transform _feedbackPos;
    [SerializeField] private bool _isChildren;
    [SerializeField] private NodeType _nodeType;
    private NodeModel _nodeModel = default;
    private float _minY = -0.5f, _maxY = 0.5f, _moveSpeed = 5f, _rotSpeed = 5f;
    private Vector2 _desintegrationVector = new Vector2(-3, 3);
    public NodeType NodeType { get { return _nodeType; } }
    public Action<NodeType> OnUpdatedNodeType = delegate { };
    #endregion

    #region VIEW
    [SerializeField] Shader _desintegrationShader;
    private Shader _originalShader;
    private NodeView _nodeView = default;
    private BoxCollider _collider = default;
    private Renderer _renderer = default;
    private Animator _animator = default;
    private ParticleSystem[] _particles = new ParticleSystem[2];
    private Color _defaultColor = Color.cyan;
    private Color _corruptionColor = Color.magenta;
    private Color _currentColor = default;
    private Outline _outline = default;
    #endregion

    private Transform _target = default;
    private IConnectable _connectable = default;
    private Vector3 _resetPos = Vector3.zero;
    public InteractablePriority Priority => InteractablePriority.Highest;
    public Color CurrentColor { get { return _currentColor; } }
   
    //metodos de la interfaz
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => true;

    //ESTO SE USA PARA SPECIFIC CONNECTION, PLANTEAR MEJOR REFACTOR
    //public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } } // Prop conectado.
    //private bool _isConnected = false;                // Flag si está conectado (no siempre usado en este fragmento).

    protected void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _renderer = GetComponentInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _outline = GetComponentInChildren<Outline>();
        _particles = GetComponentsInChildren<ParticleSystem>();
        _originalShader = _renderer.material.shader;
        _resetPos = transform.position;

        _currentColor = _nodeType == NodeType.Default ? _defaultColor : _corruptionColor;

        _nodeModel = new NodeModel(transform, _feedbackPos, _minY, _maxY, _moveSpeed, _rotSpeed);
        _nodeView = new NodeView(_renderer, _collider, _outline, _currentColor, _animator, _particles);
    }

    protected void Start()
    {
        _nodeView.OnStart(); 
        _nodeView.UpdateNodeType(_nodeType, _currentColor);
    }

    protected void Update()
    {
        if (!_isChildren) MoveObject();
        else _nodeView.SetCollectedAnim();
    }
    private void MoveObject()
    {
        _nodeView.EnableColl(true);                                 
        _nodeModel.MoveObject();                                    
    }

    public bool CanInteract(PlayerTDController player) => !player.HasNode();
    public void Interact(PlayerTDController player, out bool succeeded)
    {
        if (!CanInteract(player))
        {
            succeeded = false;
            return;
        }

        Attach(player.attachPos, player.transform, new Vector3(0.6f, 0.6f, 0.6f), parentIsPlayer: true);
        succeeded = true;
    }

    public void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false)
    {
        if (parentIsPlayer)
        {
            GlitchActive.Instance.OnChangeObjectState += InteractWithGlitcheable;
            _nodeView.EnableColl(false);
        }
        else
        {
            GlitchActive.Instance.OnChangeObjectState -= InteractWithGlitcheable;
            _nodeView.EnableColl(true);
        }

        if (!parentIsPlayer && newParent != null)
            _connectable = newParent.GetComponent<IConnectable>();

        if (newParent != null) _isChildren = true;
        else _isChildren = false;

        if (newParent != null && newScale != default)
            _nodeModel.SetPos(newPos, NodeType, newParent, newScale);
        else if (newParent != null && newScale == default)
            _nodeModel.SetPos(newPos, NodeType, newParent);
        else if (newParent == null && newScale == default)
            _nodeModel.SetPos(newPos, NodeType);
    }
    private void InteractWithGlitcheable(Glitcheable glitcheable, InteractionOutcome interactionResult)
    {
        if (glitcheable == null) return;

        bool newObjectState = _nodeType == NodeType.Default ? false : true;

        if (glitcheable.ChangeCorruptionState(_nodeType, newObjectState))
            UpdateNodeType();
    }

    private void UpdateNodeType()
    {
        _nodeType = _nodeType == NodeType.Default ? NodeType.Corrupted : NodeType.Default;
        _currentColor = _nodeType == NodeType.Default ? _defaultColor : _corruptionColor;

        _nodeView.UpdateNodeType(_nodeType, _currentColor);
        OnUpdatedNodeType?.Invoke(_nodeType);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerTDController player))
        {
            if (_target == null) _target = player.transform;

            _nodeView.SetRangeAnim();
        }
        else if (other.CompareTag("Void")) _nodeModel.ResetPos(_resetPos);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerTDController player))
        {
            if (_target != null) _target = null;      

            _nodeView.SetIdleAnim();
        }
    }

    public void StartDesintegrateShader() => _nodeView.StartDisintegrate(_desintegrationShader, _desintegrationVector);
    public void SetDesintegrateShader(float alpha) => _nodeView.SetDisintegrateAlpha(alpha);
    public void StopDesintegrateShader() => _nodeView.StopDisintegrate(_originalShader);
}
