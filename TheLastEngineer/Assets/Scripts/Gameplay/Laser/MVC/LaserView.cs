using UnityEngine;

public class LaserView : MonoBehaviour
{
    [Header("View References")]
    [SerializeField] private GameObject _lineRendererPrefab;
    [SerializeField] private ParticleSystem _hitLaser;
    [SerializeField] private ParticleSystem _beamLaser;
    [SerializeField] private ParticleSystem _Sparks;
    [SerializeField] private ParticleSystem _Smoke;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Vector3 _particleOffset;
    
    private LineRenderer _line;
    private AudioSource _audio;

    public void Init(float width)
    {
        var instance = Instantiate(_lineRendererPrefab);
        _line = instance.GetComponent<LineRenderer>();
        
        _line.positionCount = 2;
        _line.startWidth = width;
        _line.endWidth = width;

        _audio = GetComponent<AudioSource>();
    }

    public void SetLaserPositions(Vector3 start, Vector3 end)
    {
        _line.SetPosition(0, start);
        _line.SetPosition(1, end);
    }

    public void ShowHitEffect(Vector3 pos, Vector3 normal)
    {
        _trailRenderer.transform.position = pos;
        _Sparks.transform.position = pos;
        _Sparks.transform.rotation = Quaternion.LookRotation(normal);
        _Smoke.transform.position = pos + _particleOffset;
        _hitLaser.transform.position = pos;
        _hitLaser.transform.rotation = Quaternion.LookRotation(normal);
        _hitLaser.gameObject.SetActive(true);
        if (!_hitLaser.isPlaying) _hitLaser.Play(); _Sparks.Play(); _Smoke.Play();

    }

    public void StopHitEffect()
    {
        _hitLaser.gameObject.SetActive(false);
        if (_hitLaser.isPlaying) _hitLaser.Stop();
    }

    public void EnableBeam(bool enable)
    {
        _line.enabled = enable;
        if (enable && !_beamLaser.isPlaying)_beamLaser.Play();
        if (!enable && _beamLaser.isPlaying) _beamLaser.Stop();
    }

    public void PlayAudio()
    {
        if (_audio.isPlaying) return;

        _audio.pitch = Random.Range(0.9f, 1.1f);
        _audio.Play();
    }

    public void StopAudio()
    {
        if (_audio.isPlaying) _audio.Stop();
    }
}
