using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificConnectionController : Connection<SecondaryTM>
{
    [SerializeField] private NodeType _requiredType;

    [Header("MVC View")]
    private MeshRenderer _renderer = default;
    private AudioSource _audioSrc = default;

    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private AudioClip _placedClip;
    [SerializeField] private AudioClip _errorClip;
    [SerializeField] private Color _color;
    [SerializeField] private Color _secColor;

    [ColorUsage(true, true)]
    [SerializeField] private Color _fresnelColor;

    [SerializeField] private ParticleSystem _ps;

    private List<SecondaryTM> _secTaskManagers = new List<SecondaryTM>();
    private SpecificConnectionView _connectionView = default;
    private NodeController _recievedNode = default;

    private bool _isDisabled = false, _isWorking = false;

    public bool IsDisabled { get { return _isDisabled; } }
    public bool IsWorking { get { return _isWorking; } }

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _audioSrc = GetComponent<AudioSource>();

        _connectionView = new SpecificConnectionView(_requiredType, _renderer, _triggerCollider, _color, _secColor, _fresnelColor, _ps, _audioSrc);
        _connectionView.OnStart();
    }

    public override void SetSecTM(SecondaryTM secTM)
    {
        _secTaskManagers.Add(secTM);
    }

    public override bool CanInteract(PlayerTDController player)
    {
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
        if (_recievedNode.NodeType == _requiredType)
            HandleTaskManagers(false);

        _recievedNode = null;
        _connectionView.Enable(true);
        _connectionView.EnableTrigger(true);
    }

    private void CheckReceivedNode()
    {
        _connectionView.EnableTrigger(false);

        if (_recievedNode.NodeType == _requiredType) HandleRecievedNode(true, false, _placedClip);
        else
        {
            HandleRecievedNode(false, true, _errorClip);
            StartCoroutine(DisableConnection());
        }
    }

    private void HandleRecievedNode(bool isValid, bool playEffects, AudioClip clip)
    {
        _isWorking = isValid;

        if (isValid)
        {
            HandleTaskManagers(isValid);
            _connectionView.PlayClip(_placedClip, 3f);
        }
        else
            _connectionView.PlayClip(_placedClip, 1f);

        _recievedNode.IsConnected = isValid;
        _connectionView.Enable(playEffects);
        _connectionView.PlayEffect(playEffects);
    }

    private void HandleTaskManagers(bool addConnection)
    {
        if (addConnection)
        {
            if (_mainTM != null)
                MainTM.Instance?.AddConnection(_requiredType);

            if (_secTaskManagers.Count > 0)
            {
                foreach (var secTM in _secTaskManagers) secTM.AddConnection(_requiredType);
            }
        }
        else
        {
            if (_mainTM != null)
                MainTM.Instance?.RemoveConnection(_requiredType);

            if (_secTaskManagers.Count > 0)
            {
                foreach (var secTM in _secTaskManagers) secTM.RemoveConnection(_requiredType);
            }
        }
    }

    private IEnumerator DisableConnection()
    {
        _isDisabled = true;
        _connectionView.ChangeColor(Color.red);

        yield return new WaitForSeconds(3f);

        _isDisabled = false;
        _connectionView.ChangeColor(_color);
    }
}
