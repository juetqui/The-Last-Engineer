using UnityEngine;

public class AudioListenerHolder : MonoBehaviour
{
    void Update()
    {
        transform.position = PlayerTDController.Instance.transform.position;
        transform.rotation = Quaternion.identity;
    }
}
