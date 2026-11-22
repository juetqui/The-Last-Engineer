/// <summary>What a platform is doing with one station, as its scanner effects see it.</summary>
public enum StationScanState
{
    /// <summary>Powered, but the platform is neither heading to nor resting at this station.</summary>
    Idle,
    /// <summary>The platform is heading to this station.</summary>
    Targeted,
    /// <summary>The platform arrived at this station (or rests here when powered on).</summary>
    Resting,
    /// <summary>The platform has no power, and this station's lights are off.</summary>
    Unpowered,
}
