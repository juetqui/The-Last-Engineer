using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private Vector2 pitchVariation;
    
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            var randomPitch = Random.Range(pitchVariation.x, pitchVariation.y);

            _audioSource.pitch = randomPitch;
            _audioSource.Play();
        }
    }
}
