using UnityEngine;

/// <summary>
/// Carries the player on the elevator, whose root is moved by its Animator (Normal update mode).
/// </summary>
public class AttachPlayer : MonoBehaviour
{
    private Rigidbody _body;
    private PlayerController _player;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _body = KinematicBody.Ensure(gameObject);
        _lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        // The Animator moved the transform after Update. Push the pose to the kinematic body now, so next
        // frame's CharacterController.Move collides with the floor where it is, then hand the player this
        // frame's displacement (applied in its next Update).
        Vector3 position = transform.position;
        _body.position = position;

        if (_player != null)
            _player.OnPlatformMoving(position - _lastPosition);

        _lastPosition = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController player) || player == _player || player.IsDead)
            return;

        Release();
        _player = player;
        _player.OnDied += Release;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player) && player == _player)
            Release();
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Release()
    {
        if (_player != null)
            _player.OnDied -= Release;

        _player = null;
    }
}
