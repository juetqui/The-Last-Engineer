using System;
using UnityEngine;

public class PlatformTeleport : MonoBehaviour, IInteractable, IProximityListener
{
    public InteractablePriority Priority => InteractablePriority.MaxPriority;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;

    [SerializeField] private PlatformTeleport _targetPlatform;
    [SerializeField] private ParticleSystem _entrada;
    [SerializeField] private ParticleSystem _salida;
    [SerializeField] private float _heightThershold = 0.5f;

    private NodeType _requiredType = NodeType.Corrupted;
    // [SerializeField] private Renderer _renderer;

    public PlatformTeleport TargetPlatform { get { return _targetPlatform; } }
    public Vector3 TargetPos {  get; private set; }

    public Action<bool> OnPlayerStepped = delegate { };

    private void Start()
    {
        Vector3 targetPos = new Vector3(0, _heightThershold, 0) + _targetPlatform.transform.position;
        TargetPos = targetPos;
        PlayerOn(false);
        OnPlayerStepped += PlayerOn;
        //_targetPlatform.OnPlayerStepped += PlayerOn;

       // _renderer = GetComponentInChildren<Renderer>();
    }

    private void PlayerOn(bool playerStepped)
    {
        
        if (playerStepped)
        {
           _entrada.Play();
        }
        else
        {
            _entrada.Stop();
        } 
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler.CurrentType == NodeType.Corrupted && playerNodeHandler != null;

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        if (!CanInteract(playerNodeHandler))
        {
            succededInteraction = false;
            return;
        }

        succededInteraction = true;
        _entrada.Stop();
    }

    public void OnPlayerProximity(bool inRange, PlayerController player)
    {
        if (inRange)
        {
            if (player.NodeHandler.CurrentType == _requiredType)
            {
                OnPlayerStepped?.Invoke(true);
                _entrada.Play();
                TargetPlatform._salida.Play();
            }

            // Antes lo hacía OnTriggerStay: mientras el jugador está sobre la plataforma,
            // su propio PS de salida permanece apagado.
            _salida.Stop();
        }
        else
        {
            OnPlayerStepped?.Invoke(false);
            _entrada.Stop();
            TargetPlatform._salida.Stop();
        }
    }
}
