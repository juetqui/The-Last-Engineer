/// <summary>
/// Pure rules of how a platform walks its route: one step along it (ping-pong, or jumping back to the start
/// when the route loops) and which station the platform is heading to. Kept out of RouteManager so EditMode
/// tests can cover it.
/// </summary>
public static class RouteStep
{
    public const int NoStation = -1;

    /// <summary>Moves <paramref name="index"/> one point along a route of <paramref name="length"/> points.</summary>
    /// <param name="loops">True: after the last point go back to the first. False: bounce and walk back.</param>
    public static void Advance(int length, bool loops, ref int index, ref int dir)
    {
        if (length <= 1) return;

        int last = length - 1;
        int next = index + dir;

        if (next >= 0 && next <= last)
        {
            index = next;
            return;
        }

        if (loops)
        {
            index = 0;
            return;
        }

        dir = -dir;
        index += dir;
    }

    /// <summary>
    /// The first station the platform reaches from <paramref name="index"/> (that point included, since it is
    /// the one being travelled to), or <see cref="NoStation"/> when the route has none.
    /// </summary>
    public static int NextStation(bool[] isStation, int index, int dir, bool loops)
    {
        if (isStation == null || index < 0 || index >= isStation.Length) return NoStation;

        // Two full laps cover every point in both directions of a ping-pong route.
        int maxSteps = isStation.Length * 2;

        for (int step = 0; step <= maxSteps; step++)
        {
            if (isStation[index]) return index;
            Advance(isStation.Length, loops, ref index, ref dir);
        }

        return NoStation;
    }
}
