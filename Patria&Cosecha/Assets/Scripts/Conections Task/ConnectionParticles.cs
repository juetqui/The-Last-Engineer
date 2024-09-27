using UnityEngine;

public class ConnectionParticles
{
    private ParticleSystem _ps;

    public ConnectionParticles(ParticleSystem ps)
    {
        _ps = ps;
    }

    public void ActivatePSError(bool turnOnOff)
    {
        _ps.gameObject.SetActive(turnOnOff);
    }
}
