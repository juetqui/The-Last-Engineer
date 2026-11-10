/// <summary>
/// Something a laser beam can hit. Calls come in pairs: one LaserReceived when a beam starts hitting
/// and one LaserNotReceived when it stops, both with the SAME emitterState, so a receptor can keep
/// count per state. A change of the emitter's state while hitting arrives as NotReceived(old) +
/// Received(new).
/// </summary>
public interface ILaserReceptor
{
    public void LaserReceived(GlitchState emitterState);
    public void LaserNotReceived(GlitchState emitterState);
}
