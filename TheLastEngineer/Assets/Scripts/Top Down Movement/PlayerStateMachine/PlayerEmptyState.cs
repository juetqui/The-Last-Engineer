using System.Collections;
using UnityEditor.Experimental.GraphView;
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

        bool succededInteraction = default;
        interactable.Interact(_playerController, out succededInteraction);

        if (interactable is ElectricityNode node)
        {
            if (_interactionCoroutine == null)
            {
                _interactionCoroutine = _playerController.StartCoroutine(StartInteraction(node));
            }
        }
        else if (!succededInteraction)
        {
            InputManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
        }
    }

    public void Exit()
    {
        _playerController = null;
    }

    public IEnumerator StartInteraction(ElectricityNode node)
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
        }
    }
}
