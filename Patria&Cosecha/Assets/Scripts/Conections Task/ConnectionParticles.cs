using UnityEngine;

public class ConnectionParticles
{
    private ParticleSystem _ps;

    public ConnectionParticles(ParticleSystem ps)
    {
        _ps = ps;
    }

    public void OnUpdate()
    {
        var em = _ps.emission;
        em.enabled = true;

        em.rateOverTime = 0f;

        em.SetBursts(
            new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.25f, 15)
            }
        );
    }
}
