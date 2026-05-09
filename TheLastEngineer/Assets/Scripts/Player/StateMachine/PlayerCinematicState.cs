using System;
using UnityEngine;

public class PlayerCinematicState : IPlayerState
{
    private PlayerStateMachine _stateMachine;
    private PlayerController _player;
    private PlayerNodeHandler _nodeHandler;
    
    private Vector3[] _waypoints;
    private int _currentWaypointIndex;
    private float _waypointReachedThreshold = 0.5f;
    private Action _onWaypointReached;
    private Action _onSequenceComplete;
    private bool _isNavigating;

    public PlayerCinematicState(PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public void Enter(PlayerController player, PlayerNodeHandler nodeHandler)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        _player = player;
        _nodeHandler = nodeHandler;
        
        // Disable player inputs and movement
        _player.SetCanMove(false);
    }

    public void StartWaypointNavigation(Vector3[] waypoints, Action onWaypointReached, Action onSequenceComplete)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("PlayerCinematicState: No waypoints provided!");
            return;
        }

        _waypoints = waypoints;
        _currentWaypointIndex = 0;
        _onWaypointReached = onWaypointReached;
        _onSequenceComplete = onSequenceComplete;
        _isNavigating = true;
    }

    public void StopNavigation()
    {
        _isNavigating = false;
        _player.ClearCinematicMovement();
    }

    public void Tick()
    {
        if (!_isNavigating || _waypoints == null || _currentWaypointIndex >= _waypoints.Length)
            return;

        Vector3 targetWaypoint = _waypoints[_currentWaypointIndex];
        Vector3 playerPos = _player.transform.position;
        
        // Calculate direction to waypoint (only XZ plane)
        Vector3 directionToWaypoint = targetWaypoint - playerPos;
        directionToWaypoint.y = 0f;
        
        float distanceToWaypoint = directionToWaypoint.magnitude;

        // Check if we've reached the current waypoint
        if (distanceToWaypoint < _waypointReachedThreshold)
        {
            _currentWaypointIndex++;
            _onWaypointReached?.Invoke();

            // Check if we've completed the sequence
            if (_currentWaypointIndex >= _waypoints.Length)
            {
                StopNavigation();
                _onSequenceComplete?.Invoke();
                return;
            }
        }
        else
        {
            // Move towards the waypoint
            Vector3 normalizedDirection = directionToWaypoint.normalized;
            _player.SetCinematicMovement(normalizedDirection);
        }
    }

    public void Exit()
    {
        _isNavigating = false;
        _player.ClearCinematicMovement();
        _player.SetCanMove(true);
        _player = null;
        _nodeHandler = null;
        _waypoints = null;
        _onWaypointReached = null;
        _onSequenceComplete = null;
    }

    public void HandleInteraction(IInteractable interactable) 
    {
        // No interactions allowed during cinematic
    }

    public void Cancel() 
    {
        // Cannot cancel cinematics
    }
}

