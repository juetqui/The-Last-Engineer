using UnityEngine;

public class PlayPSCollisionSFX : MonoBehaviour
{
    [SerializeField] private AudioClip[] audios;
    [SerializeField] private float volume = 1.0f;
    
    private void OnParticleCollision(GameObject other)
    {
        SFXManager.Instance.PlayRandomSFX(audios, other.transform, volume);
    }
}
