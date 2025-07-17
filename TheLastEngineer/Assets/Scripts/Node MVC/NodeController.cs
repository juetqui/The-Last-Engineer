using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class NodeController : MonoBehaviour, IInteractable
{
    [Header("View")]
    [SerializeField] private Transform _feedbackPos;
    private BoxCollider _collider = default;
    private Renderer _renderer = default;
    protected Animator _animator = default;
    private Transform _target = default;
    private ParticleSystem[] _particles = new ParticleSystem[2];

    [Header("Outline")]
    [SerializeField] private Color _corruptionOutline;
    [SerializeField] private Color _defaultOutline;
    private Color _currentOutline = default;
    private Outline _outline = default;

    [Header("Model")]
    [SerializeField] protected NodeType _nodeType;
    [SerializeField] protected LayerMask _floorLayer;
    [SerializeField] private float _rotSpeed, _minY, _maxY, _moveSpeed;
    [SerializeField] private bool _isChildren;
    [SerializeField] Vector2 _desintegrationVector = new Vector2(-3, 3);
    public InteractablePriority Priority => InteractablePriority.Highest;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => true;

    private NodeView _nodeView = default;
    private NodeModel _nodeModel = default;
    private RaycastHit hit = default;

    private IConnectable _connectable = default;
    private Shader _originalShader;
    private bool _isConnected = false;
    private Vector3 _resetPos = Vector3.zero;
    [SerializeField] Shader _desintegrationShader;
    public Action<NodeType> OnUpdatedNodeType = delegate { };
    private Color _myColor;
    public NodeType NodeType { get { return _nodeType; } }
    public Color OutlineColor { get { return _currentOutline; } }
    public bool IsChildren { get { return _isChildren; } }
    public bool IsConnected { get { return _isConnected; } set { _isConnected = value; } }

    protected void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _renderer = GetComponentInChildren<Renderer>();
        _animator = GetComponent<Animator>();
        _outline = GetComponentInChildren<Outline>();
        _particles = GetComponentsInChildren<ParticleSystem>();
        _originalShader = _renderer.material.shader;
        _resetPos = transform.position;

        _currentOutline = _nodeType == NodeType.Default ? _defaultOutline : _corruptionOutline;

        _nodeModel = new NodeModel(transform, _feedbackPos, _minY, _maxY, _moveSpeed, _rotSpeed);
        _nodeView = new NodeView(_renderer, _collider, _outline, _currentOutline, _animator);
    }

    protected void Start()
    {
        _nodeView.OnStart();
        _nodeView.UpdateNodeType(_nodeType, _currentOutline);
    }
    public void StartDesintegrateShader()
    {
        _renderer.material.shader = _desintegrationShader;
        _renderer.material.shader = _desintegrationShader;
        _renderer.material.SetFloat("_ColorController", 1);
        _renderer.material.SetColor("_StartingColor", _myColor);
        _renderer.material.SetVector("_MinMaxPos", _desintegrationVector);
        _renderer.material.SetFloat("_Alpha", 1);
        _outline.enabled = false;

    }
    public void SetDesintegrateShader(float alpha)
    {
        _renderer.material.SetFloat("_Alpha", alpha);
    }
    public void StopDesintegrateShader()
    {
        //StartCoroutine(DesintegrateCo());
        _renderer.material.shader = _originalShader;
        _outline.enabled = true;

    }
    protected void Update()
    {
        //_nodeModel.RotateToTarget(_target);

        if (!_isChildren) MoveObject();
        else _nodeView.SetCollectedAnim();

        if (_target != null && _nodeType == NodeType.Corrupted)
        {
            foreach (var ps in _particles)
            {
                if (!ps.isPlaying) ps.Play();
            }
        }
        else
        {
            foreach (var ps in _particles)
            {
                if (ps.isPlaying) ps.Stop();
            }
        }

    }

    public bool CanInteract(PlayerTDController player)
    {
        return !player.HasNode();
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
        _currentOutline = _nodeType == NodeType.Default ? _defaultOutline : _corruptionOutline;

        _nodeView.UpdateNodeType(_nodeType, _currentOutline);
        OnUpdatedNodeType?.Invoke(_nodeType);
    }

    public  void Interact(PlayerTDController player, out bool succededInteraction)
    {
        if (CanInteract(player))
        {
            Attach(player, player.attachPos);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }
    }

    private void MoveObject()
    {
        _nodeView.EnableColl(true);
        //_nodeModel.MoveObject();

        if (!Physics.Raycast(transform.position, -transform.up, out hit, 3f, _floorLayer))
            transform.position -= Vector3.up * Time.deltaTime * 15f;
    }

    protected void Attach(PlayerTDController player, Vector3 newPos)
    {
        Vector3 newScale = new Vector3(0.6f, 0.6f, 0.6f);
        
        Attach(newPos, player.transform, newScale, true);

        if (_isChildren && _connectable != null)
        {
            _connectable.UnsetNode(this);
            _connectable = null;
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerTDController player))
        {
            if (_target == null)
            {
                //if (_nodeType == NodeType.Corrupted) foreach (var ps in _particles) ps.Play();
                //else foreach (var ps in _particles) ps.Stop();
                
                _target = player.transform;
            }
            
            _nodeView.SetRangeAnim();
        }
        else if (other.CompareTag("Void"))
            _nodeModel.ResetPos(_resetPos);
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.TryGetComponent(out PlayerTDController player))
    //    {
    //        if (_target != null && _nodeType == NodeType.Corrupted)
    //        {
    //            foreach (var ps in _particles)
    //            {
    //                if (!ps.isPlaying) ps.Play();
    //            }
    //        }
    //        else foreach (var ps in _particles) ps.Stop();
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerTDController player))
        {
            if (_target != null)
            {
                //if (_nodeType == NodeType.Corrupted) foreach (var ps in _particles) ps.Play();
                //else foreach (var ps in _particles) ps.Stop();
                
                _target = null;
            }

            _nodeView.SetIdleAnim();
        }
    }
}
