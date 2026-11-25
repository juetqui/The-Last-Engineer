using UnityEngine;

public class FloorChanger : MonoBehaviour
{
    [SerializeField] private FloorHeight targetFloor;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(out _)) return;

        var manager = FloorDitherManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"{name}: no hay FloorDitherManager en la escena.", this);
            return;
        }

        manager.DitherFloor(targetFloor);
    }
}
