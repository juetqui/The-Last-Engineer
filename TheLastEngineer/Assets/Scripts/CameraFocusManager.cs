using Cinemachine;
using UnityEngine;

public class CameraFocusManager : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _camera;
    [SerializeField] private Transform _newTarget;
    [SerializeField] private Transform _lookAtPoint;
    [SerializeField] private float _transitionTime = 1.2f;
    [SerializeField] private LeanTweenType _easeType = LeanTweenType.easeInOutQuad;

    private Transform _cameraTarget;
    private bool _isInZone = false;
    private float _blendValue = 0f;
    private LTDescr _currentTween;

    void Awake()
    {
        _cameraTarget = _camera.LookAt;
        _lookAtPoint.position = _cameraTarget.position;
    }

    void Update()
    {
        Vector3 midPoint = Vector3.Lerp(_cameraTarget.position, _newTarget.position, 0.5f);
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