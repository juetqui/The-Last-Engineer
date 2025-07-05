using System;
using System.Collections;
using UnityEngine;

public class PlayerTDModel
{
    private CharacterController _cc = default;
    private Transform _transform = default;
    private Collider _collider = default;

    private float _moveSpeed = default, _rotSpeed = default;
    private float _dashSpeed = default, _dashDuration = default, _dashCD = default;
    private float _coyoteTimeCounter = 0f, _coyoteTime = 0.3f;

    private float _gravity = -9.81f;
    private bool _isDashing = false, _canDash = true, _useGravity = true;


    private Vector3 _velocity = default, _platformDisplacement = Vector3.zero;

    public bool IsDashing { get { return _isDashing; } }
    public bool CanDash { get { return _canDash; } }

    public Action<float> OnDashCDStarted = delegate { };

    public PlayerTDModel(CharacterController cc, Transform transform, PlayerData playerData, Collider colider)
    {
        _cc = cc;
        _transform = transform;
        _collider = colider;
        _moveSpeed = playerData.moveSpeed;
        _rotSpeed = playerData.rotSpeed;
        _dashSpeed = playerData.dashSpeed;
        _dashDuration = playerData.dashDuration;
        _dashCD = playerData.dashCD;
    }

    public void OnUpdate(Vector3 moveDir, float moveSpeed)
    {
        _moveSpeed = moveSpeed;
        MovePlayer(moveDir);

        if (_cc.isGrounded)
            _coyoteTimeCounter = _coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;
    }

    public bool CanDashWithCoyoteTime()
    {
        return (_canDash && (_cc.isGrounded || _coyoteTimeCounter > 0f));
    }

    private void MovePlayer(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0f)
            RotatePlayer(moveDir);

        if (!_isDashing && _useGravity)
        {
            if (_cc.isGrounded)
                _velocity.y = -1f;
            else
                _velocity.y -= _gravity * -5f * Time.deltaTime;
        }
        else
        {
            _velocity.y = 0f;
        }

        Vector3 totalMovement = (moveDir.normalized * _moveSpeed * Time.deltaTime) + _platformDisplacement;
        _cc.Move(totalMovement);
        _cc.Move(_velocity * Time.deltaTime);
    }

    public void OnPlatformMoving(Vector3 displacement)
    {
        _platformDisplacement = displacement;
    }

    public void SetPos(Vector3 newPos)
    {
        bool wasEnabled = _cc.enabled;
        _cc.enabled = false;
        _transform.position = new Vector3(newPos.x, _transform.position.y, newPos.z);
        _cc.enabled = wasEnabled;
    }

    public void SetGravity(bool useGravity)
    {
        _useGravity = useGravity;
    }

    public void RotatePlayer(Vector3 rotDir)
    {
        Quaternion toRotation = Quaternion.LookRotation(rotDir.normalized, Vector3.up);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, toRotation, _rotSpeed * Time.deltaTime);
    }

    public IEnumerator Dash(Vector3 dashDir, NodeType currentNode)
    {
        float dashTimer = Time.time;
        _isDashing = true;
        _canDash = false;

        float dashSpeed = _dashSpeed;

        while (Time.time < dashTimer + _dashDuration)
        {
            _cc.Move(dashDir.normalized * dashSpeed * Time.deltaTime);
            yield return null;
        }

        _isDashing = false;
    }

    public IEnumerator DashCD()
    {
        OnDashCDStarted?.Invoke(_dashCD);
        yield return new WaitForSeconds(_dashCD);
        _canDash = true;
    }
}
