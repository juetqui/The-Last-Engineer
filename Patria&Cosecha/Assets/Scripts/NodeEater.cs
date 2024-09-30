using System.Collections;
using UnityEngine;

public class NodeEater : MonoBehaviour
{
    [SerializeField] private TaskManager _taskManager;
    [SerializeField] private ConnectionNode[] _connectionNodes = default;
    [SerializeField] private float moveSpeed = default, stoppingDistance = default, _disconnectCooldown = default;

    private ConnectionNode _targetToEat = default;
    private Vector3 _targetPos = default, _initialPos = default;
    private bool _canDisconnect = true;

    private void Awake()
    {
        _initialPos = transform.position;
    }

    void Start()
    {
        int indexToEat = Random.Range(0, _connectionNodes.Length);
        _targetToEat = _connectionNodes[indexToEat];
        _targetPos = _targetToEat.transform.position;
    }

    void Update()
    {
        if (!_taskManager.Running)
        {
            if (!_targetToEat.IsWorking)
            {
                _canDisconnect = true;
                MoveToTarget(_initialPos);
            }
            else MoveToTarget(_targetPos);
        }
    }

    private void MoveToTarget(Vector3 targetPos)
    {
        Vector3 directionToTarget = targetPos - transform.position;

        if (transform.position != _initialPos) transform.rotation = Quaternion.LookRotation(directionToTarget);

        if (directionToTarget.magnitude > stoppingDistance) transform.position += directionToTarget.normalized * moveSpeed * Time.deltaTime;
        else if (_canDisconnect && targetPos != _initialPos)
        {
            _canDisconnect = false;
            StartCoroutine(DisconnectNode());
        }
    }

    private IEnumerator DisconnectNode()
    {
        yield return new WaitForSeconds(_disconnectCooldown);
        _targetToEat.ResetNode();
    }
}
