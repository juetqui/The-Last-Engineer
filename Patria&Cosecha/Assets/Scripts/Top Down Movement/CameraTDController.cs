using UnityEngine;

public class CameraTDController : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;

    void Update()
    {
        transform.position = new Vector3(_playerTransform.position.x, transform.position.y, _playerTransform.position.z);
    }
}