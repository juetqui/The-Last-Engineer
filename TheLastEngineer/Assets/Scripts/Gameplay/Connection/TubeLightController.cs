using System.Collections.Generic;
using UnityEngine;

// Plays a row of tube lights in order when its activation source changes. The order itself
// (direction, next tube, interruptions) lives in TubeLightSequence; this only wires Unity to it.
public class TubeLightController : MonoBehaviour
{
    // Index 0 is the tube next to the source (the Connection); the last one is the far end.
    [SerializeField] private List<TubeLight> _tubes = new List<TubeLight>();
    private IActivationSource _source;
    private TubeLightSequence _sequence;

    private TubeLight _currentTube;
    private bool _isDispatching;            // inside TubeLight.PlayFill
    private bool _completedDuringDispatch;  // the tube completed synchronously (already in the asked state)

    public int Index => _sequence?.CurrentIndex ?? TubeLightSequence.NoTube;

    // Any IActivationSource on the same object drives the tubes (a Connection, a LaserReceptor...):
    // fill when it activates, empty when it deactivates. GetComponent in Awake so the first OnEnable
    // can already subscribe.
    private void Awake()
    {
        _sequence = new TubeLightSequence(_tubes?.Count ?? 0);
        _source = GetComponent<IActivationSource>();

        if (_source == null)
            Debug.LogWarning($"{nameof(TubeLightController)}: {name} has no IActivationSource; the tubes will never light up.", this);
    }

    // Subscribing in OnEnable runs before any Start, so an activation raised from a source's Start
    // (a Connection with a pre-assigned node) always arrives.
    private void OnEnable()
    {
        if (_source != null)
            _source.OnActivationChanged += OnActivationChanged;
    }

    // Disabling mid-sequence forgets it; the next activation starts at an end and the tubes already
    // in the asked state complete instantly, so nothing needs resyncing on enable.
    private void OnDisable()
    {
        if (_source != null)
            _source.OnActivationChanged -= OnActivationChanged;

        ReleaseCurrentTube();
        _sequence?.Stop();
    }

    // An interruption keeps the tube in transition: it gets the opposite PlayFill and the sequence
    // rewinds from there. Assumes no source is raised from inside a tube's callbacks (none is today).
    private void OnActivationChanged(bool active)
    {
        ReleaseCurrentTube();
        Play(_sequence.Start(active));
    }

    // Iterative on purpose: tubes already in the asked state complete synchronously inside PlayFill,
    // and recursing per tube would re-enter HandleTubeCompleted.
    private void Play(int index)
    {
        while (index != TubeLightSequence.NoTube)
        {
            if (index >= _tubes.Count)
            {
                Debug.LogWarning($"{nameof(TubeLightController)}: tube index {index} is out of range (the list changed at runtime). Sequence stopped.", this);
                _sequence.Stop();
                return;
            }

            TubeLight tube = _tubes[index];
            if (tube == null)
            {
                Debug.LogWarning($"{nameof(TubeLightController)}: TubeLight at index {index} is null. Skipped.", this);
                index = _sequence.Advance();
                continue;
            }

            _currentTube = tube;
            tube.OnTransitionCompleted += HandleTubeCompleted;

            _completedDuringDispatch = false;
            _isDispatching = true;
            tube.PlayFill(_sequence.Fill);
            _isDispatching = false;

            if (!_completedDuringDispatch) return; // wait for the tube's animation

            ReleaseCurrentTube();
            index = _sequence.Advance();
        }
    }

    private void HandleTubeCompleted(TubeLight tube)
    {
        if (tube != _currentTube) return; // stale event from a tube no longer in play

        if (_isDispatching)
        {
            _completedDuringDispatch = true; // Play's loop advances
            return;
        }

        ReleaseCurrentTube();
        Play(_sequence.Advance());
    }

    private void ReleaseCurrentTube()
    {
        if (_currentTube != null)
            _currentTube.OnTransitionCompleted -= HandleTubeCompleted;

        _currentTube = null;
    }
}
