using System.Runtime.CompilerServices;
using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Material _cutoutMat;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _targetCutoutSize = 1f;
    [SerializeField] private float _timeModifier = 0.1f;

    private Camera _mainCamera = default;
    private RaycastHit _hit;
    private bool _hasObstacle = false, _lastCheck = false;
    private float _currentSize = 0f;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        CheckCutout();
    }

    private void CheckCutout()
    {
        Vector3 offset = _target.position - transform.position;

        _hasObstacle = Physics.Linecast(transform.position, _target.position, out _hit, _layerMask);

        if (_hasObstacle != _lastCheck)
        {
            _lastCheck = _hasObstacle;
            _currentSize = _lastCheck ? 0f : _targetCutoutSize;
        }

        if (_hasObstacle)
        {
            Vector3 cutoutPos = _mainCamera.WorldToViewportPoint(_hit.point);
            cutoutPos.y /= (Screen.width / Screen.height);

            _cutoutMat.SetVector("_CutoutPos", cutoutPos);
            _cutoutMat.SetFloat("_EnableCutout", 1f);
            UpdateCutoutSize(true);
        }
        else
        {
            _cutoutMat.SetFloat("_EnableCutout", 0f);
            UpdateCutoutSize(false);
        }
    }
    private void UpdateCutoutSize(bool increment)
    {
        float delta = Time.deltaTime * _timeModifier;

        if (increment)
            _currentSize = Mathf.Min(_currentSize + delta, _targetCutoutSize);
        else
            _currentSize = Mathf.Max(_currentSize - delta, 0f);

        _cutoutMat.SetFloat("_CutoutSize", _currentSize);
    }
}
