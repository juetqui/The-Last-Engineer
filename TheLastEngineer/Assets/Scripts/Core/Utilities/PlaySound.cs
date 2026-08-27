using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float volume;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            SFXManager.Instance.PlaySFX(audioClip, transform, volume);
    }
}
