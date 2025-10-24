using UnityEngine;

public class Corruption : MonoBehaviour
{
    [SerializeField] private float _minSpeed = 2;
    [SerializeField] private float _maxSpeed = 10;
    [SerializeField] private ParticleSystem _psHitting = default;
    [SerializeField] private ParticleSystem _psRemoved = default;

    private CorruptionGenerator _generator = default;
    
    private Renderer _renderer = default;
    private Collider _collider = default;
    private Light _light = default;
    private AudioSource _audioSource = default;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
        _light = GetComponentInChildren<Light>();
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
            _psHitting.Play();
            _audioSource.Play();
        }
        else
        {
            _psHitting.Stop();
            _audioSource.Stop();
            _light.intensity = 0.1f;
        }
    }

    private void Hitting(float timer)
    {
        float amount = Mathf.Lerp(_minSpeed, _maxSpeed, timer);
        var ps = _psHitting.velocityOverLifetime;
        ps.speedModifier = amount;
        _light.intensity = timer;
    }

    private void Removed(Corruption hitted)
    {
        if (hitted != this) return;

        _generator.RemoveCorruption();
        _audioSource.Stop();
        _psHitting.Stop();
        _psRemoved.Play();
    }

    public void SetPos((int index, Vector3 position, Quaternion rotation) posData)
    {
        transform.localPosition = posData.position;
        transform.localRotation = posData.rotation;
    }

    public void TurnOnOff(bool turnOnOff)
    {
        _renderer.enabled = turnOnOff;
        _collider.enabled = turnOnOff;
        _light.enabled = turnOnOff;
    }
}
