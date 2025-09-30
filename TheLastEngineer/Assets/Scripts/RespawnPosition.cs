using UnityEngine;

public class RespawnPosition : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController> (out PlayerController playerController))
            playerController.SetCheckPointPos(transform.position);
    }
}
