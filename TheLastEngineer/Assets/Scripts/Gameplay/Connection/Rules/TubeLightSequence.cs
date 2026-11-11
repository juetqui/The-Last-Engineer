using System;

/// <summary>
/// Order in which a row of tube lights fills and drains. No Unity types so EditMode tests can
/// reach it; TubeLightController plays the tubes this returns.
/// Filling goes from the source (index 0) to the far end; draining mirrors it. Lit tubes are
/// always a prefix of the list, so an interruption keeps the tube in transition and only flips
/// the direction: the new sequence rewinds from there instead of restarting at an end.
/// </summary>
public sealed class TubeLightSequence
{
    public const int NoTube = -1;

    private const int FirstIndex = 0;

    private readonly int _tubeCount;

    public int CurrentIndex { get; private set; } = NoTube;
    public bool IsRunning => CurrentIndex != NoTube;
    public bool Fill { get; private set; }

    public TubeLightSequence(int tubeCount)
    {
        if (tubeCount < 0)
            throw new ArgumentOutOfRangeException(nameof(tubeCount), tubeCount, "Tube count can't be negative.");

        _tubeCount = tubeCount;
    }

    /// <summary>
    /// Starts filling or draining. While running, keeps the current tube and only changes the
    /// direction. Returns the index to play now, or NoTube when there are no tubes.
    /// </summary>
    public int Start(bool fill)
    {
        Fill = fill;

        if (!IsRunning && _tubeCount > 0)
            CurrentIndex = fill ? FirstIndex : _tubeCount - 1;

        return CurrentIndex;
    }

    /// <summary>
    /// The current tube finished its transition. Returns the next index, or NoTube when the
    /// sequence is over (it is then idle).
    /// </summary>
    public int Advance()
    {
        if (!IsRunning) return NoTube;

        int next = Fill ? CurrentIndex + 1 : CurrentIndex - 1;
        CurrentIndex = next >= FirstIndex && next < _tubeCount ? next : NoTube;
        return CurrentIndex;
    }

    /// <summary>Goes idle and forgets the current tube; the next Start begins at an end.</summary>
    public void Stop()
    {
        CurrentIndex = NoTube;
    }
}
