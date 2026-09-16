using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioClip exitClip;
    [SerializeField] private float volume;
    
    [SerializeField] private bool hasExitClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            SFXManager.Instance.PlaySFX(audioClip, transform, volume);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player) && hasExitClip)
            SFXManager.Instance.PlaySFX(exitClip, transform, volume);
    }
}
