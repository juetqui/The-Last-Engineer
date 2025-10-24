using UnityEngine;

public class PlayerDissolvingState : IPlayerState
{
    private PlayerStateMachine _fsm = default;
    private PlayerController _player = default;

    private bool _isDissolving = false;

    private float _timer = 0f;
    private float _dissolveDuration = 0.25f;

    public PlayerDissolvingState(PlayerStateMachine fsm)
    {
        _fsm = fsm;
    }

    public void Enter(PlayerController player, PlayerNodeHandler nodeHanlder)
    {
        _player = player;
        _isDissolving = !_isDissolving;

        if (_isDissolving)
        {
            _timer = 0f;
            _player.SetCollisions(false);
        }
        else
            _timer = 1f;

        _player.PlayTeleportPS();
    }

    public void Tick()
    {
        if (_isDissolving)
            Dissolve();
        else
            Revert();
    }

    public void Exit()
    {
        _player = null;
    }

    public void HandleInteraction(IInteractable interactable) { }

    public void Cancel() { }

    private void Dissolve()
    {
        _timer += Time.deltaTime / _dissolveDuration;
        _player.Dissolving(_timer);

        if (_timer >= 1f)
            _fsm.TransitionToTeleport();
    }

    private void Revert()
    {
        _timer -= Time.deltaTime / _dissolveDuration;
        _player.Dissolving(_timer);

        if (_timer <= 0f)
        {
            _player.SetCollisions(true);
            _fsm.TransitionToState(_fsm.LastState);
        }
    }
}
