using UnityEngine;

public class PlayerTDModel
{
    private float _moveSpeed = default, _rotSpeed = default, _dashSpeed = default, _dashDrag = default;
    private bool _isDashing = false;
    
    private Rigidbody _rb = default;
    private Transform _transform = default;
    
    private LayerMask _groundMask = default;

    public PlayerTDModel(Rigidbody rb, Transform transform, LayerMask groundMask, float moveSpeed, float rotSpeed, float dashSpeed, float dashDrag)
    {
        _rb = rb;
        _transform = transform;
        _groundMask = groundMask;
        _moveSpeed = moveSpeed;
        _rotSpeed = rotSpeed;
        _dashSpeed = dashSpeed;
        _dashDrag = dashDrag;
    }

    //void Start()
    //{
        
    //}

    public void OnUpdate(Vector3 moveDir)
    {
        CheckFloor();
        MovePlayer(moveDir);
    }

    private void CheckFloor()
    {
        Vector3 rayDir = new Vector3(_transform.position.x, _transform.position.y, _transform.position.z - 1);

        if (!_isDashing && !Physics.Raycast(rayDir, -_transform.up, 2.5f, _groundMask))
            _rb.AddForce(Vector3.down * 200);
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

    public void Dash(Vector3 moveDir)
    {
        if (_isDashing) return;

        _rb.drag = _dashDrag;

        Vector3 dir = moveDir.normalized * _dashSpeed;
        _rb.AddForce(dir, ForceMode.Impulse);
        RotatePlayer(moveDir);
        _isDashing = true;
    }

    public void EndDash()
    {
        _isDashing = false;
        _rb.drag = 0f;
    }
}
