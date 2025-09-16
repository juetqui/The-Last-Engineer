using UnityEngine;

public class PlayerDashParticlesManager : MonoBehaviour
{
    private ParticleSystem _ps = default;

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _ps.Stop();

        PlayerController.Instance.View.OnDashViewPlayed += PlayDashPS;
    }

    private void PlayDashPS()
    {
        _ps.Play();
    }
}
