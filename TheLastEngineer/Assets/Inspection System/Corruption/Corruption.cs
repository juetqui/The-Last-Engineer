using UnityEngine;

public class Corruption : MonoBehaviour
{
    [SerializeField] private float _minSpeed = 2;
    [SerializeField] private float _maxSpeed = 10;
    
    private CorruptionGenerator _generator = default;
    
    private Renderer _renderer = default;
    private Collider _collider = default;
    private Light _light = default;
    private ParticleSystem _ps = default;
    private AudioSource _audioSource = default;

    private float _maxPSAmount = 20000f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _light = GetComponentInChildren<Light>();
        _ps = GetComponentInChildren<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        CorruptionRemover.Instance.OnCorruptionHit += Hitted;
        CorruptionRemover.Instance.OnHittingCorruption += Hitting;
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
            _light.intensity = 0.1f;
        }
    }

    private void Hitting(float timer)
    {
        float amount = Mathf.Lerp(_minSpeed, _maxSpeed, timer);
        var ps = _ps.velocityOverLifetime;
        ps.speedModifier = amount;
        _light.intensity = timer;
    }

    private void Removed(Corruption hitted)
    {
        if (hitted != this) return;

        _generator.RemoveCorruption(this);
        _audioSource.Stop();
        _ps.Stop();
    }

    public void TurnOnOff(bool turnOnOff)
    {
        _renderer.enabled = turnOnOff;
        _collider.enabled = turnOnOff;
        _light.enabled = turnOnOff;
    }
}
