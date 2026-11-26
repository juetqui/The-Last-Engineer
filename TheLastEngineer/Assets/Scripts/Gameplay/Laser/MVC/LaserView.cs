using PrimeTween;
using UnityEngine;

public class LaserView : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Header("View References")]
    [SerializeField] private GameObject _lineRendererPrefab;
    [SerializeField] private ParticleSystem _hitLaser;
    [SerializeField] private ParticleSystem _beamLaser;
    [SerializeField] private ParticleSystem _Sparks;
    [SerializeField] private ParticleSystem _Smoke;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Vector3 _particleOffset;

    // The sound fade targets are the same for every laser: they come from Resources/LaserData.asset.
    private static LaserData Data => LaserData.Default;

    private const float SoundOff = 0f;
    
    private LineRenderer _line;
    private Material _lineMaterial;
    private AudioSource _audio;
    private bool _trailEmitting;
    private Sequence _soundTween;

    public void Init(float width)
    {
        // The trail starts off: the controller turns it on once the beam length is stable.
        _trailEmitting = false;
        _trailRenderer.emitting = false;

        var instance = Instantiate(_lineRendererPrefab);
        _line = instance.GetComponent<LineRenderer>();
        
        _line.positionCount = 2;
        _line.startWidth = width;
        _line.endWidth = width;

        // renderer.material makes a per-instance copy: every laser tints only its own beam.
        _lineMaterial = _line.material;

        _audio = TryGetComponent(out AudioSource source) ? source : null;
    }

    public void SetEmissionColor(Color color)
    {
        _lineMaterial.SetColor(EmissionColorId, color);
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

    // Off = stop emitting (what is already drawn fades out). On = drop the stale points first, so the
    // trail doesn't draw a streak from where it was before. Guarded so it can be called every frame.
    public void SetTrailEmitting(bool emitting)
    {
        if (_trailEmitting == emitting) return;

        _trailEmitting = emitting;
        if (emitting) _trailRenderer.Clear();
        _trailRenderer.emitting = emitting;
    }

    public void EnableBeam(bool enable)
    {
        _line.enabled = enable;
        if (enable && !_beamLaser.isPlaying)_beamLaser.Play();
        if (!enable && _beamLaser.isPlaying) _beamLaser.Stop();
    }

    // Tweens volume and pitch from their current values, so an interrupted fade reverses from where it
    // is. The duration scales with the volume left to cover. Off stops the source once it reaches 0.
    public void SetSound(bool on)
    {
        if (_audio == null) return;

        _soundTween.Stop();

        var data = Data;
        var targetVolume = on ? data.SoundMaxVolume : SoundOff;
        var targetPitch = on ? Random.Range(data.SoundMinPitch, data.SoundMaxPitch) : SoundOff;

        // Already silent and stopped: nothing to fade out.
        if (!on && !_audio.isPlaying) return;

        if (on && !_audio.isPlaying)
        {
            _audio.volume = SoundOff;
            _audio.pitch = SoundOff;
            _audio.Play();
        }

        var remaining = Mathf.Abs(targetVolume - _audio.volume);

        if (data.SoundMaxVolume <= 0f || data.SoundEaseTime <= 0f || Mathf.Approximately(remaining, 0f))
        {
            _audio.volume = targetVolume;
            _audio.pitch = targetPitch;
            if (!on) StopSource();
            return;
        }

        var duration = data.SoundEaseTime * Mathf.Clamp01(remaining / data.SoundMaxVolume);

        _soundTween = Sequence.Create()
            .Group(Tween.AudioVolume(_audio, targetVolume, duration, data.SoundEaseType))
            .Group(Tween.AudioPitch(_audio, targetPitch, duration, data.SoundEaseType));

        if (!on) _soundTween.OnComplete(this, view => view.StopSource());
    }

    private void StopSource()
    {
        if (_audio != null && _audio.isPlaying) _audio.Stop();
    }

    private void OnDestroy()
    {
        _soundTween.Stop();
    }
}
