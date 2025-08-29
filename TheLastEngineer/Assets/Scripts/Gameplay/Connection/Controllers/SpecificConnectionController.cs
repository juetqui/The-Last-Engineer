using System.Collections;
using UnityEngine;
using System;

public class SpecificConnectionController : Connection<TaskManager>
{
    [SerializeField] private NodeType _requiredType;

    [Header("MVC View")]
    private MeshRenderer _renderer;
    private AudioSource _audioSrc;

    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private AudioClip _placedClip;
    [SerializeField] private AudioClip _errorClip;
    [SerializeField] private Color _color;
    [SerializeField] private Color _secColor;
    [SerializeField] private ParticleSystem idlePSystem;
    [SerializeField] private ParticleSystem correctPSystem;
    [SerializeField] private GameObject correcNodeIndicator;

    [ColorUsage(true, true)][SerializeField] private Color _fresnelColor;
    [SerializeField] private ParticleSystem _ps;

    private SpecificConnectionView _connectionView;

    private bool _isDisabled = false, _isWorking = false;

    // El TaskManager universal se suscribe a este evento
    public Action<NodeType, bool> OnNodeConnected;

    public bool IsDisabled => _isDisabled;
    public bool IsWorking => _isWorking;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _audioSrc = GetComponent<AudioSource>();

        _connectionView = new SpecificConnectionView(
            _requiredType, _renderer, _triggerCollider,
            _color, _secColor, _fresnelColor, _ps, _audioSrc
        );
        _connectionView.OnStart();
    }

    public override void SetSecTM(TaskManager _)
    {
        // No-op: el TaskManager central se suscribe a OnNodeConnected
    }

    public override bool CanInteract(PlayerTDController player)
    {
        if (correctPSystem) correctPSystem.Stop();
        if (correcNodeIndicator) correcNodeIndicator.SetActive(false);
        if (idlePSystem) idlePSystem.Stop();
        return player.HasNode() && !_isDisabled;
    }

    protected override void SetNode(NodeController node)
    {
        if (_isDisabled || node == null) return;

        node.Attach(Vector3.zero, transform);
        _recievedNode = node;
        CheckReceivedNode();
    }

    public override void UnsetNode(NodeController node = null)
    {
        if (_recievedNode == null) return;

        if (_recievedNode.NodeType == _requiredType)
            OnNodeConnected?.Invoke(_requiredType, false);

        _recievedNode = null;
        _connectionView.Enable(true);
        _connectionView.EnableTrigger(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerTDController playerTDController)) return;

        bool hasRequired = playerTDController.HasNode() &&
                           playerTDController.GetCurrentNode().NodeType == _requiredType;

        if (hasRequired)
        {
            if (idlePSystem) { idlePSystem.Stop(); idlePSystem.Clear(); }
            if (correctPSystem) correctPSystem.Play();
            if (correcNodeIndicator) correcNodeIndicator.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerTDController _)) return;

        if (correctPSystem) correctPSystem.Stop();
        if (correcNodeIndicator) correcNodeIndicator.SetActive(false);
        if (idlePSystem) idlePSystem.Play();
    }

    private void CheckReceivedNode()
    {
        _connectionView.EnableTrigger(false);

        if (_recievedNode.NodeType == _requiredType)
        {
            HandleReceivedNode(true, false);
        }
        else
        {
            HandleReceivedNode(false, true);
            StartCoroutine(DisableConnection());
        }
    }

    private void HandleReceivedNode(bool isValid, bool playEffects)
    {
        _isWorking = isValid;

        if (isValid)
        {
            OnNodeConnected?.Invoke(_requiredType, true);
            _connectionView.PlayClip(_placedClip, 3f);
        }
        else
        {
            _connectionView.PlayClip(_errorClip ? _errorClip : _placedClip, 1f);
        }

        _recievedNode.IsConnected = isValid;
        _connectionView.Enable(playEffects);
        _connectionView.PlayEffect(playEffects);
    }

    private IEnumerator DisableConnection()
    {
        _isDisabled = true;
        _connectionView.ChangeColor(Color.red);

        yield return new WaitForSeconds(3f);

        _isDisabled = false;
        _connectionView.ChangeColor(_color);
        _connectionView.EnableTrigger(true);
    }
}
