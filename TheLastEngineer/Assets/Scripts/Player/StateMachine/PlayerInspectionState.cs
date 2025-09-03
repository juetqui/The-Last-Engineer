using System;
using UnityEngine;

public class PlayerInspectionState : MonoBehaviour, IPlayerState
{
    private PlayerTDController _player = default;
    private Inspectionable _inspectionable = default;

    public Action<Inspectionable> OnTargetSelected;

    public void Enter(PlayerTDController player)
    {
        _player = player;
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (interactable == null || !(Inspectionable)interactable || !interactable.CanInteract(_player)) return;

        _inspectionable = (Inspectionable)interactable;
        OnTargetSelected?.Invoke(_inspectionable);
    }
    
    public void Tick() { }

    public void Cancel() { }

    public void Exit()
    {
        OnTargetSelected?.Invoke(null);
        _player.SetState(_player.LastState);
        
        _inspectionable = null;
        _player = null;
    }
}
