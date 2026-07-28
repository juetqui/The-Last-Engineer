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
        // Sin CheckForWalls: ese raycast miraba hacia transform.forward sin importar dónde estaba
        // el objetivo. El gate de pared ahora es la línea de visión por objetivo, que ya filtró
        // este interactuable en InteractableHandler.GetInteractable.
        if (interactable == null) return;

        _target = interactable;
        
        if (interactable.RequiresHoldInteraction)
        {
            _holding = true;
            _holdTimer = 0f;
        }
        else
        {
            TryDoInteraction(_target);
        }
    }

    private void TryDoInteraction(IInteractable interactable)
    {
        bool success;
        interactable.Interact(_playerNodeHandler, out success);

        if (!success) return;

        if (interactable is NodeController node)
        {
            _player.RemoveInteractable(node);
            _stateMachine.TransitionToGrabState(node);
            return;
        }

        if (interactable is PlatformTeleport teleport)
        {
            _player.SetPos(teleport.TargetPos);
            _player.RemoveInteractable(interactable);
            InputManager.Instance?.RumblePulse(0.25f, 1f, 0.25f);
        }

        if (interactable is Inspectionable inspectionable)
        {
            inspectionable.OnFinished += RemoveInteractable;
        }
    }

    public void Tick()
    {
        if (!_holding || _target == null) return;

        // Si aparece una pared en el medio del hold, se corta como si el jugador soltara el botón:
        // el LOS se recalcula por frame, así que la interacción nunca se completa a través de ella.
        if (!_player.HasLineOfSight(_target))
        {
            Cancel();
            return;
        }

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

    private void RemoveInteractable()
    {
        if (_target is Inspectionable inspectionable)
            inspectionable.OnFinished -= RemoveInteractable;

        _player.RemoveInteractable(_target);
    }
}
