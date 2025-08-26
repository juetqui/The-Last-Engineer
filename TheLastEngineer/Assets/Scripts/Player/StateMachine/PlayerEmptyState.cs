using UnityEngine;

public class PlayerEmptyState : IPlayerState
{
    private PlayerTDController _player;
    private float _holdTimer;
    private bool _holding;
    private IInteractable _target;

    public void Enter(PlayerTDController player)
    {
        _player = player;
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
        else
        {
            TryDoInteraction(interactable);
        }
    }

    private void TryDoInteraction(IInteractable interactable)
    {
        bool success;
        interactable.Interact(_player, out success);
        if (success && interactable is NodeController node)
        {
            _player.PickUpNode(node);
            _player.SetState(_player.GrabState);
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
        if (_holding)
        {
            _holding = false;
            _target = null;
            InputManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
        }
    }

    public void Exit()
    {
        _player = null;
        _target = null;
        _holding = false;
    }
}
