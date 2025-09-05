using UnityEngine;

public class Corruption : MonoBehaviour
{
    private CorruptionGenerator _generator = default;
    private ParticleSystem _ps = default;
    private AudioSource _audioSource = default;

    void Start()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
        CorruptionRemover.Instance.OnCorruptionHit += Hitted;
        CorruptionRemover.Instance.OnCorruptionRemoved += Removed;
    }

    public void SetUpGenerator(CorruptionGenerator generator)
    {
        _generator = generator;
    }

    private void Hitted(Corruption hitted)
    {
        if (hitted == this)
        {
            _ps.Play();
            _audioSource.Play();
        }
        else
        {
            _ps.Stop();
            _audioSource.Stop();
        }
    }
    private void Removed(Corruption hitted)
    {
        if (hitted != this) return;

        _generator.RemoveCorruption(this);
        _audioSource.Stop();
        _ps.Stop();
    }
}
