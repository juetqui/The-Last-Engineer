using System.Collections;
using UnityEngine;

public class PlayerTDModel
{
    private CharacterController _cc = default;
    private Transform _transform = default;

    private float _moveSpeed = default, _rotSpeed = default;
    private float _dashSpeed = default, _dashDuration = default, _dashCD = default;

    private float _gravity = -9.81f, _dashTimer = 0f;
    private float _rayDistance = 0.5f, _rayOffset = 0.5f;
    private bool _isDashing = false, _canDash = true;

    private Vector3 _velocity = default;

    public bool IsDashing { get { return _isDashing; } }
    public bool CanDash { get { return _canDash; } }

    public delegate void OnDashCDFinished();
    public OnDashCDFinished onDashCDFinished = default;

    public PlayerTDModel(CharacterController cc, Transform transform, float moveSpeed, float rotSpeed, float dashSpeed, float dashDuration, float dashCD)
    {
        _cc = cc;
        _transform = transform;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
        _dashCD = dashCD;
    }

    public void OnUpdate(Vector3 moveDir, float moveSpeed)
    {
        _moveSpeed = moveSpeed;
        MovePlayer(moveDir);
    }

    private void MovePlayer(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0.1f)
            RotatePlayer(moveDir);

        if (!_isDashing)
        {
            if (_cc.isGrounded)
                _velocity.y = -1f;
            else
                _velocity.y -= _gravity * -2f * Time.deltaTime;
        }

        _cc.Move(moveDir.normalized * _moveSpeed * Time.deltaTime);
        _cc.Move(_velocity * Time.deltaTime);
    }

    private void RotatePlayer(Vector3 rotDir)
    {
        Quaternion toRotation = Quaternion.LookRotation(rotDir.normalized, Vector3.up);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, toRotation, _rotSpeed * Time.deltaTime);
    }

    public IEnumerator Dash(Vector3 dashDir, NodeType currentNode)
    {
        float dashTimer = Time.time;
        _isDashing = true;
        _canDash = false;

        float dashSpeed = currentNode == NodeType.Blue ? _dashSpeed : _dashSpeed * 1.25f;

        while (Time.time < dashTimer + _dashDuration)
        {
            _cc.Move(dashDir.normalized * dashSpeed * Time.deltaTime);
            yield return null;
        }
        
        _isDashing = false;
    }

    public IEnumerator DashCD()
    {
        yield return new WaitForSeconds(_dashCD);
        onDashCDFinished?.Invoke();
        _canDash = true;
    }
}
