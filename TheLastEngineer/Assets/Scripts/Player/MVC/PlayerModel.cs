using System;
using System.Collections;
using UnityEngine;

public class PlayerModel
{
    private CharacterController _cc = default;
    private Transform _transform = default;
    private Collider _collider = default;

    private float _moveSpeed = default, _rotSpeed = default;
    private float _dashSpeed = default, _dashDuration = default, _dashCD = default;
    private float _coyoteTimeCounter = 0f, _coyoteTime = default;

    private float _gravity = -50f;
    private bool _isDashing = false, _canDash = true;


    private Vector3 _velocity = default, _platformDisplacement = Vector3.zero;

    public bool IsDashing { get { return _isDashing; } }
    public bool CanDash { get { return _canDash; } }

    public Action<float> OnDashCDStarted = delegate { };

    public PlayerModel(CharacterController cc, Transform transform, PlayerData playerData, Collider colider)
    {
        if (cc == null || transform == null || playerData == null)
            throw new System.ArgumentNullException("Dependences can not be null");

        _cc = cc;
        _transform = transform;
        _collider = colider;
        _moveSpeed = playerData.moveSpeed;
        _rotSpeed = playerData.rotSpeed;
        _dashSpeed = playerData.dashSpeed;
        _dashDuration = playerData.dashDuration;
        _dashCD = playerData.dashCD;
        _coyoteTime = playerData.coyoteTime;
    }

    public void OnUpdate(Vector3 moveDir, float moveSpeed)
    {
        _moveSpeed = moveSpeed;
        MovePlayer(moveDir);
        UpdateCoyoteTimer();
    }

    private void MovePlayer(Vector3 moveDir)
    {
        if (_isDashing) return;
        //if (_isDashing) moveDir *= 0.01f;

        if (moveDir.sqrMagnitude > 0.0001f)
            RotatePlayer(moveDir);


        Vector3 horizontal = GetHorizontalMovement(moveDir);
        Vector3 vertical = HandleVerticalMovement();
        Vector3 totalMovement = horizontal + vertical + _platformDisplacement;

        _cc.Move(totalMovement);
        _platformDisplacement = Vector3.zero;
    }
    
    private Vector3 GetHorizontalMovement(Vector3 moveDir)
    {
        return moveDir.normalized * _moveSpeed * Time.deltaTime;
    }

    private Vector3 HandleVerticalMovement()
    {
        if (!_isDashing)
        {
            if (_cc.isGrounded) _velocity.y = -1f;
            else _velocity.y += _gravity * Time.deltaTime;
        }
        else _velocity.y = 0f;

        return _velocity * Time.deltaTime;
    }

    private void UpdateCoyoteTimer()
    {
        _coyoteTimeCounter = _cc.isGrounded ? _coyoteTime : _coyoteTimeCounter - Time.deltaTime;
    }

    public void OnPlatformMoving(Vector3 displacementPerFrame)
    {
        _platformDisplacement = displacementPerFrame;
    }


    public bool CanDashWithCoyoteTime()
    {
        return (_canDash && (_cc.isGrounded || _coyoteTimeCounter > 0f));
    }

    public void SetPos(Vector3 newPos)
    {
        bool wasEnabled = _cc.enabled;
        _cc.enabled = false;
        _transform.position = newPos;
        //_transform.position = new Vector3(newPos.x, _transform.position.y, newPos.z);
        _cc.enabled = wasEnabled;
    }

    public void SetRespawnPos(Vector3 respawnPos)
    {
        bool wasEnabled = _cc.enabled;
        _cc.enabled = false;
        _transform.position = respawnPos;
        _cc.enabled = wasEnabled;
    }

    public void RotatePlayer(Vector3 rotDir)
    {
        Quaternion toRotation = Quaternion.LookRotation(rotDir.normalized, Vector3.up);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, toRotation, _rotSpeed * Time.deltaTime);
    }

    public IEnumerator Dash(Vector3 dashDir)
    {
        if (dashDir == Vector3.zero)
            dashDir = _transform.forward;

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
