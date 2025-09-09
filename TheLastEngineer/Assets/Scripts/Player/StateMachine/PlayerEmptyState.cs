using UnityEngine;

public class PlayerEmptyState : IPlayerState
{
    private PlayerStateMachine _stateMachine;
    private PlayerController _player;
    private PlayerNodeHandler _playerNodeHandler;
    private float _holdTimer;
    private bool _holding;
    private IInteractable _target;

    public PlayerEmptyState(PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public void Enter(PlayerController player, PlayerNodeHandler playerNodeHandler)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        _player = player;
        _playerNodeHandler = playerNodeHandler;
        _holding = false;
        _target = null;
        _holdTimer = 0f;
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (interactable == null || _player.CheckForWalls()) return;

        _target = interactable;
        
        if (interactable.RequiresHoldInteraction)
        {
            _holding = true;
            _holdTimer = 0f;
        }
        else TryDoInteraction(interactable);
    }

    private void TryDoInteraction(IInteractable interactable)
    {
        bool success;
        interactable.Interact(_playerNodeHandler, out success);

        if (!success)
        {
            Debug.Log("FAIL TO GRAB");
            return;
        }

        if (interactable is NodeController node)
        {
            _player.RemoveInteractable(node);
            _stateMachine.TransitionToGrabState(node);
            return;
        }

        if (interactable is PlatformTeleport teleport)
        {
            _player.SetTeleport(teleport.TargetPos);
            _player.RemoveInteractable(interactable);
            InputManager.Instance?.RumblePulse(0.25f, 1f, 0.25f);
        }
    }

    public void Tick()
    {
        if (!_holding || _target == null) return;

        _holdTimer += Time.deltaTime;
        
        if (_holdTimer >= _player.GetHoldInteractionTime())
        {
            _holding = false;
            TryDoInteraction(_target);
            _target = null;
        }
    }

    public void Cancel()
    {
        if (!_holding) return;

        _holding = false;
        _target = null;
        InputManager.Instance?.RumblePulse(0.25f, 1f, 0.25f);
    }

    public void Exit()
    {
        _player = null;
        _playerNodeHandler = null;
        _target = null;
        _holding = false;
    }
}
