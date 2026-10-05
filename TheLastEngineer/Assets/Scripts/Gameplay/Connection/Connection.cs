using System;
using UnityEngine;

public class Connection : MonoBehaviour, IInteractable, IConnectable, IProximityListener, IActivationSource
{
    #region -----INTERFACE VARIABLES-----
    public InteractablePriority Priority => InteractablePriority.Medium;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;
    #endregion

    [SerializeField] private NodeController _recievedNode;
    [SerializeField] private Transform _nodePos;
    [SerializeField] private GlitchState _requiredLevel = GlitchState.Clean;
    //[SerializeField] private GameObject _particleNode;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionOff;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionCorrect;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionIncorrect;
    
    [SerializeField] private float connectDelay = 0.25f;

    private Renderer _renderer = default;
    
    private float _timer = 0f;

    // Se guarda en vez de calcularse contra _recievedNode.Level: el nivel del nodo puede cambiar
    // mientras esta conectado, y las vistas necesitan saber lo que se ANUNCIO por OnNodeConnected
    // para sincronizarse sin depender del orden de ejecucion.
    private bool _isCorrectlyConnected = false;

    public GlitchState RequiredLevel {  get { return _requiredLevel; } }
    public bool StartsConnected { get; private set; }
    public bool IsConnected => _recievedNode != null;
    public bool IsCorrectlyConnected => _isCorrectlyConnected;
    public bool IsActive => _isCorrectlyConnected;

    public Action OnInitialized;
    public Action<GlitchState, bool> OnNodeConnected;
    public Action<bool> OnAvailableToConnect;
    public event Action<bool> OnActivationChanged;

    private void Start()
    {
        //_particleNode.SetActive(true);
        _renderer = GetComponentInParent<Renderer>();
        _renderer.material.SetColor("_EmissiveColor", _emissionOff);
        OnInitialized?.Invoke();

        if (_recievedNode != null)
        {
            SetNode(_recievedNode);
            StartsConnected = true;
        }
        else StartsConnected = false;
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler.HasNode && _recievedNode == null;

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        if (_recievedNode != null)
        {
            succededInteraction = false;
        }
        else if (CanInteract(playerNodeHandler))
        {
            NodeController node = playerNodeHandler.CurrentNode;
            SetNode(node);
            succededInteraction = true;
        }
        else
        {
            succededInteraction = false;
        }
    }

    private void SetNode(NodeController node)
    {
        node.Attach(_nodePos.localPosition, transform, Vector3.one * 0.15f, false, _nodePos.rotation);
        _recievedNode = node;

        if (_recievedNode.Level == _requiredLevel)
        {
            _isCorrectlyConnected = true;
            OnNodeConnected?.Invoke(node.Level, true);
            OnActivationChanged?.Invoke(true);
            _renderer.material.SetColor("_EmissiveColor", _emissionCorrect);
            //_particleNode.SetActive(false);
        }
        else
        {
            _renderer.material.SetColor("_EmissiveColor", _emissionIncorrect);
        }

    }

    public void UnsetNode(NodeController node)
    {
        bool wasCorrect = _isCorrectlyConnected;

        // El estado se limpia antes de avisar para que quien consulte IsConnected /
        // IsCorrectlyConnected desde el callback ya vea la conexion libre.
        _isCorrectlyConnected = false;
        _recievedNode = null;
        _renderer.material.SetColor("_EmissiveColor", _emissionOff);
        //_particleNode.SetActive(true);

        // Un nodo incorrecto nunca anuncio la conexion, asi que tampoco anuncia la desconexion:
        // el evento siempre llega en pares true/false y las vistas no apagan algo que no prendieron.
        // Se anuncia con _requiredLevel (el nivel con el que se anuncio el true) y no con node.Level,
        // que pudo cambiar mientras estaba conectado y dejaria a DoorsController sin descontar.
        if (wasCorrect)
        {
            OnNodeConnected?.Invoke(_requiredLevel, false);
            OnActivationChanged?.Invoke(false);
        }
    }

    public void OnPlayerProximity(bool inRange, PlayerController player)
    {
        if (inRange)
        {
            if (player.NodeHandler.HasNode && player.NodeHandler.CurrentLevel == _requiredLevel && !IsConnected)
                OnAvailableToConnect?.Invoke(true);
        }
        else
        {
            OnAvailableToConnect?.Invoke(false);
        }
    }
}