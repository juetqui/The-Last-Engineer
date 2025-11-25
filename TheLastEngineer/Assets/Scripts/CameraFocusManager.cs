using Cinemachine;
using UnityEngine;

public class CameraFocusManager : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _camera;
    [SerializeField] private Transform _newTarget;
    [SerializeField] private float _verticalOffset = 10f;
    [SerializeField] private float _transitionTime = 1.2f;
    [SerializeField] private LeanTweenType _easeType = LeanTweenType.easeInOutSine;

    private Transform _cameraTarget = default;
    private Transform _lookAtPoint = default;
    private LTDescr _currentTween = null;
    private bool _isInZone = false;
    private float _blendValue = 0f;

    void Awake()
    {
        _lookAtPoint = new GameObject(gameObject.name + "'s Look At Point").transform;

        _cameraTarget = _camera.LookAt;
        _lookAtPoint.position = _cameraTarget.position;
    }

    void LateUpdate()
    {
        if (!_isInZone) return;

        Vector3 midPoint = (_cameraTarget.position + _newTarget.position) * 0.5f;
        midPoint -= Vector3.up * _verticalOffset;
        _lookAtPoint.position = Vector3.Lerp(_cameraTarget.position, midPoint, _blendValue);
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            _isInZone = true;
            _camera.LookAt = _lookAtPoint;

            if (_currentTween != null)
                LeanTween.cancel(gameObject);

            _currentTween = LeanTween.value(gameObject, _blendValue, 1f, _transitionTime)
                .setEase(_easeType)
                .setOnUpdate((float val) => _blendValue = val);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            _isInZone = false;

            if (_currentTween != null)
                LeanTween.cancel(gameObject);

            _currentTween = LeanTween.value(gameObject, _blendValue, 0f, _transitionTime)
                .setEase(_easeType)
                .setOnUpdate((float val) => _blendValue = val)
                .setOnComplete(() => _camera.LookAt = _cameraTarget);
        }
    }
}