using UnityEngine;

public class FloorChanger : MonoBehaviour
{
    [SerializeField] private FloorHeight targetFloor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var playerController))
        {
            FloorDitherManager.Instance.DitherFloor(targetFloor);
        }
    }
}
