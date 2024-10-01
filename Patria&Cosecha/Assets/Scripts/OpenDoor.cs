using System.Collections;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    [SerializeField] private Transform _openPos = default, _closePos = default;
    private bool _isOpen = true, _open = false, _canFinishClose = false;
    private float _stopDist = 0.01f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) _open = true;
        
        if (_open)
        {
            //MoveDoor(_closePos.position);
            if (_isOpen) MoveDoor(_closePos.position);
            else MoveDoor(_openPos.position);
        }
    }

    private void MoveDoor(Vector3 targetPos)
    {
        Vector3 preTarget = new Vector3(transform.position.x, transform.position.y, targetPos.z);
        Vector3 preDir = preTarget - transform.position;
        if (preDir.magnitude > _stopDist)
        {
            transform.position += preDir.normalized * 5f * Time.deltaTime;
            StartCoroutine(CloseDelay());
        }

        if (_canFinishClose)
        {
            Vector3 target = new Vector3(targetPos.x, transform.position.y, transform.position.z);
            Vector3 dir = target - transform.position;
            if (dir.magnitude > _stopDist)
            {
                transform.position += dir.normalized * 5f * Time.deltaTime;
                _canFinishClose = false;
                _isOpen = !_isOpen;
                //_open = false;
            }
        }
    }

    private IEnumerator CloseDelay()
    {
        yield return new WaitForSeconds(1f);
        _canFinishClose = true;
    }
}
