using UnityEngine;

public class PlayerTDModel
{
    private float _moveSpeed = default, _rotSpeed = default, _dashSpeed = default, _dashDrag = default, _upgradedDashSpeed = default, _dashCooldown = default, _dashTimer = default;
    private bool _isDashing = false;
    private Vector3 _oldScale = default;
    
    private Rigidbody _rb = default;
    private Transform _transform = default;
    
    private LayerMask _groundMask = default;

    public bool IsDashing {  get { return _isDashing; } }
    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float DashSpeed { get { return _dashSpeed; } set { _dashSpeed = value; } }
    public float DashDrag { get { return _dashDrag; } set { _dashDrag = value; } }

    public PlayerTDModel(Rigidbody rb, Transform transform, LayerMask groundMask, float moveSpeed, float rotSpeed, float dashSpeed, float dashDrag, float dashCooldown)
    {
        _rb = rb;
        _transform = transform;
        _groundMask = groundMask;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
        _dashSpeed = dashSpeed;
        _upgradedDashSpeed = _dashSpeed + 20f;
        _dashDrag = dashDrag;
        _dashCooldown = dashCooldown;
    }

    public void OnStart()
    {
        _oldScale = _transform.localScale;
        
    }

    public void OnUpdate(Vector3 moveDir, float deltaTime)
    {
        MovePlayer(moveDir);
    }

    private void MovePlayer(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0.1f) RotatePlayer(moveDir);

        Vector3 dir = moveDir.normalized * _moveSpeed;
        
        _rb.velocity = new Vector3(dir.x, _rb.velocity.y, dir.z);
    }

    private void RotatePlayer(Vector3 rotDir)
    {
        Quaternion toRotation = Quaternion.LookRotation(rotDir.normalized, Vector3.up);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, toRotation, _rotSpeed * Time.deltaTime);
    }

    public void Dash(NodeType nodeType, Vector3 moveDir)
    {
        if (_isDashing) return;

        RotatePlayer(moveDir);

        Vector3 dir = moveDir.normalized * (nodeType == NodeType.Dash ? _upgradedDashSpeed : _dashSpeed);

        _rb.useGravity = false;
        _rb.drag = _dashDrag;
        _rb.AddForce(dir, ForceMode.VelocityChange);
        
        _isDashing = true;
        _dashTimer = _dashCooldown;
    }

    public void UpdateDashTimer(float deltaTime)
    {
        if (_dashTimer > 0f && _isDashing) _dashTimer -= deltaTime;
        else EndDash();
    }

    private void EndDash()
    {
        _isDashing = false;
        _rb.useGravity = true;
        _rb.drag = 0f;
    }
}
