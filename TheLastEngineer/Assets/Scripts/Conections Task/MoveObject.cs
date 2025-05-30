using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveObject : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private Color _outlineColor;

    private Vector3 _originalPos = default, _currentPos = default, _targetPos = default;
    private Quaternion _originalRot = default, _currentRot = default, _targetRot = default;

    private Outline _outline = default;
    private Collider _collider = default;
    private string _tag = "";
    private bool _isMoving = false, _toggle = false;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _originalPos = transform.position;
        _originalRot = transform.rotation;
        _tag = gameObject.tag;
    }

    void Start()
    {
        _outline = gameObject.AddComponent<Outline>();
        _outline.OutlineColor = _outlineColor;
        _outline.OutlineWidth = 3;

        NodeEffectController.Instance.OnToggleObjects += SetTarget;
        ActiveObjectMove.Instance.OnSelectionActivated += ActivateSelection;
        ActiveObjectMove.Instance.OnObjectSelected += CheckSelected;
    }

    private void ActivateSelection(bool isSelecting)
    {
        _outline.OutlineWidth = 3;

        if (isSelecting)
            _outline.OutlineColor = Color.yellow;
        else
            _outline.OutlineColor = _outlineColor;
    }

    private void CheckSelected(MoveObject targetObject)
    {
        if (targetObject != this)
        {
            _outline.OutlineWidth = 3;
            _outline.OutlineColor = Color.yellow;
            return;
        }

        _outline.OutlineColor = Color.green;
        _outline.OutlineWidth = 7;
    }

    public void TogglePos()
    {
        _toggle = !_toggle;
        SetTarget(_toggle);
    }

    private void SetTarget(bool toggle)
    {
        _toggle = toggle;
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

        StartCoroutine(MoveToTarget());
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
                //_collider.enabled = true;
                gameObject.tag = _tag;
                
                yield break;
            }

            yield return null;
        }
    }
}
