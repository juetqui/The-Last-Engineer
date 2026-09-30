using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlayPSCollisionSFX : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private AudioClip[] audios;
    [SerializeField] private float volume = 1.0f;
    // Los rebotes llegan con mucha menos energía que el primer impacto: debajo de este umbral no suenan
    [SerializeField] private float minImpactSpeed = 2f;
    // Un burst grande puede impactar casi en simultáneo; se limita para no instanciar decenas de AudioSources
    [SerializeField] private int maxSoundsPerFrame = 2;

    [Header("Burst")]
    [SerializeField] private AudioClip burstClip;
    [SerializeField] private float burstVolume = 1.0f;

    private ParticleSystem _ps;
    private readonly List<ParticleCollisionEvent> _events = new List<ParticleCollisionEvent>();
    private int _soundsThisFrame;
    private int _lastSoundFrame = -1;
    private int _lastParticleCount;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (audios == null || audios.Length == 0) return;

        if (_lastSoundFrame != Time.frameCount)
        {
            _lastSoundFrame = Time.frameCount;
            _soundsThisFrame = 0;
        }

        var count = _ps.GetCollisionEvents(other, _events);
        for (int i = 0; i < count && _soundsThisFrame < maxSoundsPerFrame; i++)
        {
            var evt = _events[i];
            if (evt.velocity.magnitude < minImpactSpeed) continue;

            // Se usa el punto de impacto y no el transform del collider golpeado (ej: el pivot del piso)
            SFXManager.Instance.PlayRandomSFX(audios, evt.intersection, volume);
            _soundsThisFrame++;
        }
    }

    // Unity no expone un evento de burst: como el sistema emite solo por bursts (rateOverTime = 0),
    // un aumento del conteo de partículas indica que salió uno. Calcularlo por ps.time no sirve con
    // probability < 1 porque no sabe si el burst realmente se emitió.
    private void LateUpdate()
    {
        var particleCount = _ps.particleCount;

        if (burstClip != null && particleCount > _lastParticleCount)
            SFXManager.Instance.PlaySFX(burstClip, transform.position, burstVolume, true, 50f);

        _lastParticleCount = particleCount;
    }
}
