using UnityEngine;

public class LaserView : MonoBehaviour
{
    [Header("View References")]
    [SerializeField] private GameObject _lineRendererPrefab;
    [SerializeField] private ParticleSystem _hitLaser;
    [SerializeField] private ParticleSystem _beamLaser;
    
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
        _hitLaser.transform.position = pos;
        _hitLaser.transform.rotation = Quaternion.LookRotation(normal);
        if (!_hitLaser.isPlaying) _hitLaser.Play();
    }

    public void StopHitEffect()
    {
        if (_hitLaser.isPlaying) _hitLaser.Stop();
    }

    public void EnableBeam(bool enable)
    {
        _line.enabled = enable;
        if (enable && !_beamLaser.isPlaying) _beamLaser.Play();
        if (!enable && _beamLaser.isPlaying) _beamLaser.Stop();
    }

    public void PlayAudio()
    {
        if (!_audio.isPlaying) _audio.Play();
    }

    public void StopAudio()
    {
        if (_audio.isPlaying) _audio.Stop();
    }
}
