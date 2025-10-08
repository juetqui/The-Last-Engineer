using System;
using UnityEngine;

public class NodeController : MonoBehaviour, IInteractable
{
    #region INTERFACE VARIABLES
    public InteractablePriority Priority => InteractablePriority.High;
    public Color CurrentColor { get { return _currentColor; } }
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    [SerializeField] private NodeType _nodeType;
    public NodeType NodeType { get { return _nodeType; } }
    public Action<NodeType> OnUpdatedNodeType = delegate { };

    #region VIEW
    [Header("VIEW")]
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

    [Header("SHADER")]
    [SerializeField] Shader _desintegrationShader;
    private Vector2 _desintegrationVector = new Vector2(-3, 3);
    private Shader _originalShader;

    private NodeModel _nodeModel = default;
    private PlayerNodeHandler _playerNodeHandler = null;
    private Transform _target = default;
    private IConnectable _connectable = default;
    private Vector3 _resetPos = Vector3.zero;
    private bool _isChildren = false;


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

        _nodeModel = new NodeModel(transform);
        _nodeView = new NodeView(_renderer, _collider, _outline, _currentColor, _animator, _particles);
    }

    protected void Start()
    {
        _nodeView.OnStart(); 
        _nodeView.UpdateNodeType(_nodeType, _currentColor);
        OnUpdatedNodeType?.Invoke(_nodeType);
    }

    protected void Update()
    {
        if (!_isChildren) _nodeView.EnableColl(true);
        else _nodeView.SetCollectedAnim();
    }
    
    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler != null && !playerNodeHandler.HasNode;
    
    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succeeded)
    {
        if (!CanInteract(playerNodeHandler))
        {
            _playerNodeHandler = null;
            succeeded = false;
            return;
        }

        _playerNodeHandler = playerNodeHandler;

        Vector3 newScale = Vector3.one  * 0.75f;
        Attach(Vector3.zero, playerNodeHandler.AttachTransform, newScale, parentIsPlayer: true);
        succeeded = true;
    }

    public void Attach(Vector3 newPos, Transform newParent = null, Vector3 newScale = default, bool parentIsPlayer = false, Quaternion newRot = default)
    {
        if (parentIsPlayer)
        {
            _playerNodeHandler.OnGlitchChange += InteractWithGlitcheable;
            _nodeView.EnableColl(false);
            _nodeView.EnableOutline(false);
        }
        else
        {
            if (_playerNodeHandler != null)
            {
                _playerNodeHandler.OnGlitchChange -= InteractWithGlitcheable;
                _playerNodeHandler = null;
            }
            
            _nodeView.EnableColl(true);
            _nodeView.EnableOutline(true);
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
            _nodeModel.SetPos(newPos, NodeType, newParent, newScale, newRot);
        else if (newParent != null && newScale == default)
            _nodeModel.SetPos(newPos, NodeType, newParent);
        else if (newParent == null && newScale == default)
            _nodeModel.SetPos(newPos, NodeType);
    }
    
    private void InteractWithGlitcheable(Glitcheable glitcheable)
    {
        if (glitcheable == null) return;
        
        UpdateNodeType();

        //bool succceededInteraction = false;
        //glitcheable.Interact();

        //if (glitcheable.Interrupt(_nodeType))
        //{
        //    UpdateNodeType();
        //    return true;
        //}
        //else return false;
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
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            if (_target == null) _target = player.transform;

            _nodeView.SetRangeAnim();
        }
        else if (other.CompareTag("Void")) _nodeModel.ResetPos(_resetPos);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            if (_target != null) _target = null;      

            _nodeView.SetIdleAnim();
        }
    }

    public void StartDesintegrateShader() => _nodeView.StartDisintegrate(_desintegrationShader, _desintegrationVector);
    public void SetDesintegrateShader(float alpha) => _nodeView.SetDisintegrateAlpha(alpha);
    public void StopDesintegrateShader() => _nodeView.StopDisintegrate(_originalShader);
}
