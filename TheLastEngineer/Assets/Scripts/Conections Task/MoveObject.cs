using System.Collections;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] Transform _targetTransform;
    [SerializeField] float _moveSpeed = 20f;
    [SerializeField] bool _debug = false;

    private Vector3 _originalPos = default, _currentPos = default, _targetPos = default;
    private Quaternion _originalRot = default, _currentRot = default, _targetRot = default;
    
    private string _tag = "";
    private bool _isMoving = false, _toggle = false, _shouldMove = false;

    private void Awake()
    {
        _originalPos = transform.position;
        _originalRot = transform.rotation;
        _tag = gameObject.tag;
    }

    void Start()
    {
        NodeEffectController.Instance.OnMoveWorld += SetTarget;
    }

    void Update()
    {
        if (!_isMoving && _shouldMove)
        {
            StartCoroutine(MoveToTarget());
        }
    }

    private void SetTarget(bool toggle)
    {
        _toggle = toggle;
        _shouldMove = true;
        _isMoving = false;

        if (toggle)
        {
            _currentPos = _originalPos;
            _currentRot = _originalRot;
            
            _targetPos = _targetTransform.position;
            _targetRot = _targetTransform.rotation;
        }
        else
        {
            _currentPos = _targetTransform.position;
            _currentRot = _targetTransform.rotation;

            _targetPos = _originalPos;
            _targetRot = _originalRot;
        }
    }

    private IEnumerator MoveToTarget()
    {
        _isMoving = true;
        gameObject.tag = "Untagged";

        while (_isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPos,
                Time.deltaTime * _moveSpeed
            );

            float initialDistance = Vector3.Distance(_currentPos, _targetPos);
            float currentDistance = Vector3.Distance(transform.position, _targetPos);
            float t = initialDistance > 0.1f ? 1f - (currentDistance / initialDistance) : 1f;

            transform.rotation = Quaternion.Slerp(
                _currentRot,
                _targetRot,
                t
            );

            if (currentDistance < 0.1f && Quaternion.Angle(transform.rotation, _targetRot) < 1f)
            {
                transform.position = _targetPos;
                transform.rotation = _targetRot;

                _isMoving = false;
                _shouldMove = false;
                gameObject.tag = _tag;
                
                yield break;
            }

            yield return null;
        }
    }
}
