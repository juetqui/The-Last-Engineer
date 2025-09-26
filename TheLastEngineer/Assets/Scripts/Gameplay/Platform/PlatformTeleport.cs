using System;
using UnityEngine;

public class PlatformTeleport : MonoBehaviour, IInteractable
{
    public InteractablePriority Priority => InteractablePriority.MaxPriority;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;

    [SerializeField] private PlatformTeleport _targetPlatform;
    [SerializeField] private ParticleSystem _entrada;
    [SerializeField] private ParticleSystem _salida;

    // [SerializeField] private Renderer _renderer;

    public PlatformTeleport TargetPlatform { get { return _targetPlatform; } }

    public Vector3 TargetPos {  get; private set; }

    public Action<bool> OnPlayerStepped = delegate { };

    private void Start()
    {
        TargetPos = _targetPlatform.transform.position;
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

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            OnPlayerStepped?.Invoke(true);
            _entrada.Play();
            TargetPlatform._entrada.Play();

        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            OnPlayerStepped?.Invoke(false);
            _entrada.Stop();
            TargetPlatform._entrada.Stop();
        }
    }
}
