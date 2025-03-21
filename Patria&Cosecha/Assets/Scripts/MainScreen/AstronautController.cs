using System;
using System.Collections;
using UnityEngine;

public class AstronautController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform[] _targetPos;
    [SerializeField] private bool _hasTarget = false, _isExit;

    [SerializeField] private float _moveSpeed;

    private Array _anims = default;
    private Transform _currentTarget = default;
    private int _index = -1;
    private bool _isAnimating = false;

    void Start()
    {
        _anims = Enum.GetValues(typeof(Animations));
        _currentTarget = _targetPos[0];
    }

    void Update()
    {
        if (!_isAnimating && !_hasTarget) StartCoroutine(SetRandomAnim());
        else if (_hasTarget && !_isExit) Move();
        else if (_hasTarget && _isExit) Move(1);
    }

    private void Move(int buttom = 0)
    {
        if (_animator == null) return;
        if (_isAnimating) _isAnimating = false;

        if (Vector3.Distance(transform.position, UpdateTargetPos()) > 0.1f)
        {
            _animator.SetBool("IsWalking", true);
            
            Vector3 dir = (UpdateTargetPos() - transform.position).normalized;

            Quaternion toRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, _moveSpeed * Time.deltaTime);

            transform.position += dir * _moveSpeed * Time.deltaTime;
        }
        else
        {
            _index++;

            if (_index == _targetPos.Length)
            {
                _hasTarget = false;
                _animator.SetBool("IsWalking", false);
                return;
            }
            if(buttom == 1 && _index == 1)
                _currentTarget = _targetPos[_index + 1];
            else
                _currentTarget = _targetPos[_index];
        }
    }

    private Vector3 UpdateTargetPos()
    {
        return new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
    }

    public void SetTarget(bool exit = false)
    {
        _hasTarget = true;
        _isExit = exit;
    }

    private IEnumerator SetRandomAnim()
    {
        _isAnimating = true;

        int randomAnim = UnityEngine.Random.Range(0, _anims.Length);
        
        yield return new WaitForSeconds(7f);

        if (randomAnim == 0) _animator.SetTrigger("Flip");
        else if (randomAnim == 1) _animator.SetTrigger("Jump");
        else if (randomAnim == 2) _animator.SetTrigger("Fall");
        else if (randomAnim == 3) _animator.SetTrigger("Surprise");

        _isAnimating = false;
    }

    private enum Animations
    {
        Flip,
        Jump,
        Fall,
        Surprise
    }
}
