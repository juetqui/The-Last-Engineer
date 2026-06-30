using UnityEngine;

public class AudioListenerHolder : MonoBehaviour
{
    [SerializeField] private Transform _cameraTarget;
    
    private Quaternion _startRot = default;
    private Vector3 _startPos = new Vector3 (0, 1, 0);
    
    private void Start()
    {
        _startRot = Quaternion.identity;
        transform.SetParent(PlayerController.Instance.transform);
        transform.localPosition = _startPos;
    }

    void Update()
    {
        transform.forward = _cameraTarget.forward;
    }
}
