using Unity.Cinemachine;
using UnityEngine;
using PrimeTween;

public class CameraFocusManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private Transform _newTarget;
    [SerializeField] private float _verticalOffset = 10f;
    [SerializeField] private float _transitionTime = 1.2f;
    [SerializeField] private Ease _easeType = Ease.InOutSine;

    private Transform _cameraTarget;
    private Transform _lookAtPoint;
    private Tween _currentTween;
    private bool _isInZone = false;
    private bool _isTransitioning = false;
    private float _blendValue = 0f;

    void Awake()
    {
        _lookAtPoint = new GameObject(gameObject.name + "'s Look At Point").transform;

        _cameraTarget = _camera.LookAt;
        _lookAtPoint.position = _cameraTarget.position;
    }

    void LateUpdate()
    {
        if (!_isInZone && !_isTransitioning) return;

        Vector3 midPoint = (_cameraTarget.position + _newTarget.position) * 0.5f;
        midPoint -= Vector3.up * _verticalOffset;
        _lookAtPoint.position = Vector3.Lerp(_cameraTarget.position, midPoint, _blendValue);
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            _isInZone = true;
            _isTransitioning = false;
            _camera.LookAt = _lookAtPoint;

            _currentTween.Stop();

            _currentTween = Tween.Custom(gameObject, _blendValue, 1f, _transitionTime,
                (_, val) => _blendValue = val, _easeType);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            _isInZone = false;
            _isTransitioning = true;

            _currentTween.Stop();

            _currentTween = Tween.Custom(gameObject, _blendValue, 0f, _transitionTime,
                (_, val) => _blendValue = val, _easeType)
                .OnComplete(() =>
                {
                    _camera.LookAt = _cameraTarget;
                    _isTransitioning = false;
                });
        }
    }

    private void OnDestroy()
    {
        _currentTween.Stop();
    }
}