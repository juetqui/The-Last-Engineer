using System.Collections;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] private Transform _movedPos;
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private string _temporaryTag = "Untagged";
    [SerializeField] private bool _debug = false;

    private Vector3 _originalPos = default, _currentPos = default, _targetPos = default;
    private Quaternion _originalRot = default, _currentRot = default, _targetRot = default;

    private Collider _collider = null;
    private string _originalTag = "";

    private bool _isMoving = false, _move = false;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _originalTag = gameObject.tag;
        
        _originalPos = transform.position;
        _originalRot = transform.rotation;
    }

    void Start()
    {
        NodeEffectController.Instance.OnToggleObjects += SetPositions;
    }

    void Update()
    {
        if (_move && !_isMoving)
        {
            StartCoroutine(MoveToTarget());
        }
    }

    private void SetPositions(bool toggle)
    {
        _move = true;

        if (toggle)
        {
            _currentPos = _originalPos;
            _currentRot = _originalRot;

            _targetPos = _movedPos.position;
            _targetRot = _movedPos.rotation;
        }
        else
        {
            _currentPos = _movedPos.position;
            _currentRot = _movedPos.rotation;

            _targetPos = _originalPos;
            _targetRot = _originalRot;
        }
    }

    private IEnumerator MoveToTarget()
    {
        _isMoving = true;
        bool keepMoving = true;
        gameObject.tag = _temporaryTag;
        _collider.enabled = false;

        while (keepMoving)
        {

            float initialDistance = Vector3.Distance(_currentPos, _targetPos);
            float currentDistance = Vector3.Distance(transform.position, _targetPos);
            float t = initialDistance > 0.001f ? 1f - (currentDistance / initialDistance) : 1f;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPos,
                Time.deltaTime * _moveSpeed
            );

            transform.rotation = Quaternion.Slerp(
                _currentRot,
                _targetRot,
                t
            );

            if (Vector3.Distance(_currentPos, _targetPos) <= 0.1f)
            {
                keepMoving = false;
                transform.position = _targetPos;
                transform.rotation = _targetRot;

                yield break;
            }

            yield return null;
        }

        _isMoving = false;
        _move = false;
        gameObject.tag = _originalTag;
        _collider.enabled = true;
    }
}
