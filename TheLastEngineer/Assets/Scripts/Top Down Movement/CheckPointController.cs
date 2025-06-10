using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController player))
        {
            Vector3 checkPointPos = player.transform.position;
            player.SetCheckPointPos(checkPointPos);
        }
    }
}
