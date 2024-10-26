using System.Collections;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [SerializeField] private Transform _openPos;
    [SerializeField] private TaskManager _taskManager;
    [SerializeField] private bool _isMenu;

    private bool _canOpen = false, _isMoving = false;
    private float _stopDist = 0.01f, _speed = 2f;
    private Vector3 _closedPos = default;

    private void Awake()
    {
        _closedPos = transform.position;
    }

    void Update()
    {
        if (_taskManager == null && _isMenu) _canOpen = true;
        else if (_taskManager != null && _taskManager.Running) _canOpen = true;
        else _canOpen = false;

        if (_canOpen && !_isMoving) StartCoroutine(Open(_openPos.position));
        else if (!_canOpen && !_isMoving) StartCoroutine(Open(_closedPos));
    }

    public void Open()
    {
        _isMenu = true;
    }

    private IEnumerator Open(Vector3 targetPos)
    {
        _isMoving = true;

        while (Mathf.Abs(transform.position.x - targetPos.x) > _stopDist)
        {
            Vector3 newPosX = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, transform.position.z), (_speed / 4) * Time.deltaTime);
            transform.position = newPosX;

            yield return null;
        }

        while (Mathf.Abs(transform.position.z - targetPos.z) > _stopDist)
        {
            Vector3 newPosZ = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, targetPos.z), _speed * Time.deltaTime);
            transform.position = newPosZ;

            yield return null;
        }

        while (Mathf.Abs(transform.position.y - targetPos.y) > _stopDist)
        {
            Vector3 newPosX = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetPos.y, transform.position.z), _speed * Time.deltaTime);
            transform.position = newPosX;

            yield return null;
        }

        _isMoving = false;
    }
}
