using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class CinematicSequence
{
    [Header("Waypoints")]
    [Tooltip("Waypoints for player to walk to BEFORE reaching the trigger")]
    public Transform[] waypointsToTrigger;
    
    [Tooltip("Waypoints for player to walk to AFTER animation completes")]
    public Transform[] waypointsAfterAnimation;

    [Header("Camera Settings")]
    public CinemachineCamera cinematicCamera;
    public CinemachineCamera gameplayCamera;
    public int cinematicCameraPriority = 20;
    public int gameplayCameraPriority = 10;

    [Header("Player Data Reference")]
    public PlayerData playerData;
}

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance { get; private set; }

    [SerializeField] private CinematicSequence _currentSequence;

    // Cinematic Events
    public event Action OnCinematicStarted;
    public event Action OnCinematicEnded;
    public event Action OnRequestEnterCinematicState;
    public event Action OnRequestExitCinematicState;

    private PlayerController _player;
    private bool _isPlayingCinematic = false;
    private bool _waitingForAnimation = false;
    private bool _playerCCWasEnabled = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartCinematic()
    {
        if (_isPlayingCinematic)
        {
            Debug.LogWarning("CinematicManager: A cinematic is already playing!");
            return;
        }

        if (_currentSequence == null)
        {
            Debug.LogError("CinematicManager: No cinematic sequence assigned!");
            return;
        }

        _player = PlayerController.Instance;

        if (_player == null)
        {
            Debug.LogError("CinematicManager: PlayerController not found!");
            return;
        }

        StartCoroutine(ExecuteCinematicSequence());
    }

    public void StartCinematic(CinematicSequence sequence)
    {
        _currentSequence = sequence;
        StartCinematic();
    }

    private IEnumerator ExecuteCinematicSequence()
    {
        _isPlayingCinematic = true;

        // Notify cinematic started
        OnCinematicStarted?.Invoke();

        // Switch to cinematic camera
        SetCameraPriorities(true);

        // Request player to transition to cinematic state
        OnRequestEnterCinematicState?.Invoke();

        // Navigate to trigger waypoints
        if (_currentSequence.waypointsToTrigger != null && _currentSequence.waypointsToTrigger.Length > 0)
        {
            Vector3[] waypointPositions = GetWaypointPositions(_currentSequence.waypointsToTrigger);

            bool reachedTrigger = false;

            _player.StateMachine.CinematicState.StartWaypointNavigation(
                waypointPositions,
                () => { }, // onWaypointReached callback
                () => { reachedTrigger = true; } // onSequenceComplete callback
            );

            // Wait until player reaches the trigger
            yield return new WaitUntil(() => reachedTrigger);
        }

        // Wait for trigger to be activated and animation to play
        // The animation will call OnAnimationComplete() via AnimationEvent
        yield return new WaitUntil(() => !_waitingForAnimation && !_isPlayingCinematic);
    }

    public void OnTriggerReached(PlayerController player, bool ccWasEnabled)
    {
        _player = player;
        _playerCCWasEnabled = ccWasEnabled;
        _waitingForAnimation = true;

        // Stop current waypoint navigation
        if (_player.StateMachine.CinematicState != null)
        {
            _player.StateMachine.CinematicState.StopNavigation();
        }
    }

    /// <summary>
    /// Called from AnimationEvent on the parent object's animation
    /// </summary>
    public void OnAnimationComplete()
    {
        if (!_waitingForAnimation) return;

        _waitingForAnimation = false;
        StartCoroutine(ContinueAfterAnimation());
    }

    private IEnumerator ContinueAfterAnimation()
    {
        // Restore player physics and hierarchy
        RestorePlayerPhysics();

        // Navigate to exit waypoints
        if (_currentSequence.waypointsAfterAnimation != null && _currentSequence.waypointsAfterAnimation.Length > 0)
        {
            Vector3[] waypointPositions = GetWaypointPositions(_currentSequence.waypointsAfterAnimation);
            
            bool reachedEnd = false;
            _player.StateMachine.CinematicState.StartWaypointNavigation(
                waypointPositions,
                () => { }, // onWaypointReached callback
                () => { reachedEnd = true; } // onSequenceComplete callback
            );

            // Wait until player reaches the end
            yield return new WaitUntil(() => reachedEnd);
        }

        // End the cinematic
        EndCinematic();
    }

    private void RestorePlayerPhysics()
    {
        if (_player == null || _currentSequence.playerData == null) return;

        // Request player to restore its physics and hierarchy
        _player.OnCinematicRestoreRequested?.Invoke(
            _currentSequence.playerData.defaultLayer, 
            _playerCCWasEnabled
        );
    }

    private void EndCinematic()
    {
        // Restore camera priorities
        SetCameraPriorities(false);

        // Request player to transition back to previous state
        OnRequestExitCinematicState?.Invoke();

        // Notify cinematic ended
        OnCinematicEnded?.Invoke();

        _isPlayingCinematic = false;
        _player = null;
    }

    private void SetCameraPriorities(bool isCinematic)
    {
        if (_currentSequence.cinematicCamera != null && _currentSequence.gameplayCamera != null)
        {
            if (isCinematic)
            {
                _currentSequence.cinematicCamera.GetComponent<Animator>().SetTrigger("isCinematic");
                
                _currentSequence.cinematicCamera.Priority = _currentSequence.cinematicCameraPriority;
                _currentSequence.gameplayCamera.Priority = _currentSequence.gameplayCameraPriority;
            }
            else
            {
                _currentSequence.gameplayCamera.Priority = _currentSequence.cinematicCameraPriority;
                _currentSequence.cinematicCamera.Priority = _currentSequence.gameplayCameraPriority;
            }
        }
    }

    private Vector3[] GetWaypointPositions(Transform[] waypoints)
    {
        Vector3[] positions = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
                positions[i] = waypoints[i].position;
        }
        return positions;
    }

    public bool IsPlayingCinematic()
    {
        return _isPlayingCinematic;
    }
}

