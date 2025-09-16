using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlesFeedbackManager : MonoBehaviour
{
    [SerializeField] private bool _startsEnabled = false;

    private List<ParticleSystem> _particles;

    private void Start()
    {
        _particles = GetComponentsInChildren<ParticleSystem>().ToList();

        if (_startsEnabled) StartParticles();
        else StopParticles();
    }

    public void StartParticles()
    {
        foreach (var ps in _particles)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
    }

    public void StopParticles()
    {
        foreach (var ps in _particles)
        {
            ps.gameObject.SetActive(false);
            ps.Stop();
        }
    }
}
