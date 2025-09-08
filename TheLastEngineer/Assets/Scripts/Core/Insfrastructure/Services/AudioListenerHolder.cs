using UnityEngine;

public class AudioListenerHolder : MonoBehaviour
{
    private Quaternion _startRot = default;
    private void Awake()
    {
        _startRot = transform.localRotation;
    }
    void Update()
    {
        transform.position = PlayerController.Instance.transform.position;
        transform.rotation = _startRot;
    }
}
