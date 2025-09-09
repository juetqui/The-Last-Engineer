using System;
using UnityEngine;

public class PlatformTeleport : MonoBehaviour, IInteractable
{
    public InteractablePriority Priority => InteractablePriority.Villero;
    public Transform Transform => transform;
    public bool RequiresHoldInteraction => false;

    [SerializeField] private Renderer _renderer;
    [SerializeField] private PlatformTeleport _targetPlatform;

    public Vector3 TargetPos {  get; private set; }

    public Action<bool> OnPlayerStepped = delegate { };

    private void Start()
    {
        TargetPos = _targetPlatform.transform.position;
        PlayerOn(false);
        OnPlayerStepped += PlayerOn;
        _targetPlatform.OnPlayerStepped += PlayerOn;
    }

    private void PlayerOn(bool playerStepped)
    {
        if (playerStepped)
            _renderer.material.SetFloat("_TRansparencyREduction", 0f);
        else
            _renderer.material.SetFloat("_TRansparencyREduction", 1f);
    }

    public bool CanInteract(PlayerNodeHandler playerNodeHandler) => playerNodeHandler != null;

    public void Interact(PlayerNodeHandler playerNodeHandler, out bool succededInteraction)
    {
        if (!CanInteract(playerNodeHandler))
        {
            succededInteraction = false;
            return;
        }
        
        succededInteraction = true;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            OnPlayerStepped?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            OnPlayerStepped?.Invoke(false);
        }
    }
}
