using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class AstronautController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform[] _targetPos;
    [SerializeField] private bool _hasTarget = false;

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
        else if (_hasTarget) Move();
    }

    private void Move()
    {
        if (Vector3.Distance(transform.position, UpdateTargetPos()) > 0.1f)
        {
            _animator.SetBool("IsWalking", true);
            
            Vector3 dir = (UpdateTargetPos() - transform.position).normalized;

            Quaternion toRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 5f * Time.deltaTime);

            transform.position += dir * 5 * Time.deltaTime;
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
            
            _currentTarget = _targetPos[_index];
        }
    }

    private Vector3 UpdateTargetPos()
    {
        return new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
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
