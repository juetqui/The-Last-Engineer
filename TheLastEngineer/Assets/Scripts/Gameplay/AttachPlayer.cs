using UnityEngine;

public class AttachPlayer : MonoBehaviour
{
    private IMovablePassenger _player = null;
    private Vector3 _lastPosition = Vector3.zero;
    private bool _hasPlayerOn = false;

    private void LateUpdate()
    {
        if (!_hasPlayerOn || _player == null)
            return;

        Vector3 displacement = transform.position - _lastPosition;

        if (displacement.sqrMagnitude > 0.0001f)
            _player.OnPlatformMoving(displacement);

        _lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IMovablePassenger player))
        {
            _player = player;
            _hasPlayerOn = true;
            _lastPosition = transform.position;
        }
    }
}
