using UnityEngine;

public class AttachToObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController passenger))
        {
            Debug.Log(passenger);
            passenger.transform.SetParent(transform);
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController passenger))
        {
            passenger.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerTDController passenger))
        {
            passenger.transform.SetParent(null);
        }
    }
}
