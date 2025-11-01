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

    private const float CheckInterval = 0.05f;

    private float _nextCheckTime;
    private float _aspectRatio;

    private Vector3 _hitPoint;
    private Vector3 _desiredCutoutPos;
    private bool _desiredEnabled;

    private Vector3 _lastCutoutPos;
    private float _lastCutoutSize;
    private bool _cutoutEnabled;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
        _aspectRatio = (float)Screen.width / Screen.height;
    }

    private void LateUpdate()
    {
        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + CheckInterval;
            SampleObstacle();
        }

        bool sizeChanged = UpdateCutoutSizeTowardsTarget();

        bool positionChanged = _desiredEnabled && (_desiredCutoutPos != _lastCutoutPos);
        bool enabledChanged = (_desiredEnabled != _cutoutEnabled);
        bool anyChange = enabledChanged || positionChanged || sizeChanged;

        if (anyChange)
        {
            ApplyCutout(_desiredCutoutPos, _currentSize, _desiredEnabled);
        }
    }

    private void SampleObstacle()
    {
        _hasObstacle = Physics.Linecast(transform.position, _target.position, out _hit, _layerMask);

        if (_hasObstacle != _lastCheck)
        {
            _lastCheck = _hasObstacle;
            _currentSize = _lastCheck ? 0f : _targetCutoutSize;
        }

        if (_hasObstacle)
        {
            _hitPoint = _hit.point;

            Vector3 vp = _mainCamera.WorldToViewportPoint(_hitPoint);
            vp.y /= _aspectRatio;

            _desiredCutoutPos = vp;
            _desiredEnabled = true;
        }
        else
        {
            _desiredCutoutPos = _lastCutoutPos;
            _desiredEnabled = (_currentSize > 0f);
        }
    }

    private bool UpdateCutoutSizeTowardsTarget()
    {
        float target = _hasObstacle ? _targetCutoutSize : 0f;
        float before = _currentSize;

        float delta = Time.deltaTime * _timeModifier;
        if (_hasObstacle)
            _currentSize = Mathf.Min(_currentSize + delta, target);
        else
            _currentSize = Mathf.Max(_currentSize - delta, target);

        if (!_hasObstacle && _currentSize <= 0f)
            _desiredEnabled = false;

        return !Mathf.Approximately(before, _currentSize);
    }

    private void ApplyCutout(Vector3 pos, float size, bool enabled)
    {
        if (enabled != _cutoutEnabled)
        {
            _cutoutEnabled = enabled;
            _cutoutMat.SetFloat("_EnableCutout", enabled ? 1f : 0f);
        }

        if (!enabled)
            return;

        if (pos != _lastCutoutPos)
        {
            _cutoutMat.SetVector("_CutoutPos", pos);
            _lastCutoutPos = pos;
        }

        if (!Mathf.Approximately(size, _lastCutoutSize))
        {
            _cutoutMat.SetFloat("_CutoutSize", size);
            _lastCutoutSize = size;
        }
    }

}
