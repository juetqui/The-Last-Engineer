using UnityEngine;
using System;

public class CinematicTrigger : MonoBehaviour
{
    [Header("Parent Settings")]
    [SerializeField] private Transform _parentObject;
    [SerializeField] private Animator _parentAnimator;
    [SerializeField] private string _animationTriggerName = "PlayCinematic";
    
    [Header("Layer Settings")]
    [SerializeField] private LayerMask _cinematicLayer;

    private bool _hasBeenTriggered = false;
    private bool _playerCCWasEnabled = false;

    public Action OnAnimationCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasBeenTriggered) return;

        if (other.TryGetComponent(out PlayerController player))
        {
            _hasBeenTriggered = true;
            ExecuteCinematicTrigger(player);
        }
    }

    private void ExecuteCinematicTrigger(PlayerController player)
    {
        // Store CharacterController state
        _playerCCWasEnabled = player.CC.enabled;

        // Request player to setup for cinematic (parenting, physics, layer)
        player.OnCinematicSetupRequested?.Invoke(_parentObject, _cinematicLayer, true);

        // Trigger the parent animation
        if (_parentAnimator != null && !string.IsNullOrEmpty(_animationTriggerName))
        {
            _parentAnimator.SetTrigger(_animationTriggerName);
        }

        // Notify CinematicManager
        if (CinematicManager.Instance != null)
        {
            CinematicManager.Instance.OnTriggerReached(player, _playerCCWasEnabled);
        }
    }

    public void ResetTrigger()
    {
        _hasBeenTriggered = false;
    }

    private void AnimationEnded()
    {
        CinematicManager.Instance.OnAnimationComplete();
    }
}

