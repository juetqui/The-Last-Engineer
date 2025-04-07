using System.Collections;
using UnityEngine;

public class PlayerEmptyState : IPlayerState
{
    private PlayerTDController _playerController = default;
    private Coroutine _interactionCoroutine = null;
    
    private float _interactionTimer = 0f;

    public void Enter(PlayerTDController playerController)
    {
        _playerController = playerController;
        _playerController.CheckCurrentNode();
    }

    public void HandleInteraction(IInteractable interactable)
    {
        if (interactable == null) return;

        if (interactable is NodeController node && _interactionCoroutine == null && !_playerController.CheckForWalls(node))
        {
            _interactionCoroutine = _playerController.StartCoroutine(StartInteraction(node));
        }
        else
        {
            bool succededInteraction = default;
            interactable.Interact(_playerController, out succededInteraction);
        }
    }

    public void Exit()
    {
        _playerController = null;
    }

    public IEnumerator StartInteraction(NodeController node)
    {
        _interactionTimer = 0f;

        while (_interactionTimer < _playerController.GetHoldInteractionTime())
        {
            _interactionTimer += Time.deltaTime;
            yield return null;
        }

        bool succededInteraction = default;
        node.Interact(_playerController, out succededInteraction);
        
        if (succededInteraction)
        {
            _playerController.PickUpNode(node);
            _playerController.SetState(_playerController.GrabState);
        }

        _interactionCoroutine = null;
    }

    public void CancelInteraction()
    {
        if (_interactionCoroutine != null)
        {
            _playerController.StopCoroutine(_interactionCoroutine);
            _interactionCoroutine = null;

            InputManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
        }
    }
}
