using System.Collections;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [SerializeField] private Transform _openPos;
    [SerializeField] private bool _isMenu;
    [SerializeField] private float _speed = 5f;

    private MainTM _mainTM = default;
    private SecondaryTM _secTM = default;
    private bool _canOpen = false, _isMoving = false;
    private float _stopDist = 0.01f;
    private Vector3 _closedPos = default;

    private bool HasMainTM { get { return _mainTM != null && _mainTM.Running; } }
    private bool HasSecTM { get { return _secTM != null && _secTM.Running; } }
    private bool IsMenu {  get { return _isMenu && !HasMainTM && !HasSecTM; } }

    private void Awake()
    {
        _closedPos = transform.position;
    }

    void Update()
    {
        if (HasMainTM || HasSecTM || IsMenu) _canOpen = true;
        else _canOpen = false;

        if (_canOpen && !_isMoving) StartCoroutine(Open(_openPos.position));
        else if (!_canOpen && !_isMoving) StartCoroutine(Open(_closedPos));
    }

    private void OpenDoors(bool isRunning)
    {
        _canOpen = isRunning;
    }

    public void SetMainTM()
    {
        _mainTM = MainTM.Instance;
        _mainTM.onRunning += OpenDoors;
    }

    public void SetSecTM(SecondaryTM taskManager)
    {
        _secTM = taskManager;
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
