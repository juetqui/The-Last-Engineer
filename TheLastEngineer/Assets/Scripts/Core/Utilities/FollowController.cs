using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;

public class FollowController : MonoBehaviour
{
    [SerializeField] private Transform lookAtObject;
    [SerializeField] private float easeTime = 0.75f;

    private CinemachineCamera _camera;
    private CinemachineOrbitalFollow _orbitalFollow;
    private PlayerController _player;

    private bool _canRotate = true;
    private bool _isAnimating;

    public float CurrentYaw => _orbitalFollow.HorizontalAxis.Value;
    private float _elapsedTime;
    private float _startCameraAngle;
    private float _targetCameraAngle;
    private float _startObjectAngle;
    private float _targetObjectAngle;

    private void Start()
    {
        _camera = GetComponent<CinemachineCamera>();
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _player = PlayerController.Instance;

        _player.OnDied += StopFollowing;
        _player.OnRespawned += StartFollowing;
        
        InputManager.Instance.cameraRight.started += RotateRight;
        InputManager.Instance.cameraLeft.started += RotateLeft;

        StartFollowing();
    }

    private void StopFollowing()
    {
        _camera.Follow = null;
        StartCoroutine(StopLooking());
    }

    private void StartFollowing()
    {
        _camera.Follow = lookAtObject;
        _camera.LookAt = lookAtObject;
    }

    private IEnumerator StopLooking()
    {
        yield return new WaitForSeconds(1f);
        _camera.LookAt = null;
    }

    private void RotateRight(InputAction.CallbackContext context)
    {
        OrientateCamera(_orbitalFollow.HorizontalAxis.Value - 90f);
    }
    
    private void RotateLeft(InputAction.CallbackContext context)
    {
        OrientateCamera(_orbitalFollow.HorizontalAxis.Value + 90f);
    }

    private void OrientateCamera(float angle)
    {
        if (!_canRotate) return;

        _startCameraAngle = _orbitalFollow.HorizontalAxis.Value;
        _targetCameraAngle = angle;
        _startObjectAngle = lookAtObject.eulerAngles.y;
        _targetObjectAngle = angle;
        _elapsedTime = 0f;
        _canRotate = false;
        _isAnimating = true;
    }

    private void Update()
    {
        if (!_isAnimating) return;

        _elapsedTime += Time.deltaTime;

        var t = Mathf.Clamp01(_elapsedTime / easeTime);
        var easedT = EaseInOutSine(t);

        UpdateCameraOrientation(Mathf.Lerp(_startCameraAngle, _targetCameraAngle, easedT));
        UpdateObjectOrientation(Mathf.Lerp(_startObjectAngle, _targetObjectAngle, easedT));

        if (t < 1f) return;

        UpdateCameraOrientation(_targetCameraAngle);
        UpdateObjectOrientation(_targetObjectAngle);
        _isAnimating = false;
        _canRotate = true;
    }

    private static float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
    }

    private void UpdateObjectOrientation(float angle)
    {
        lookAtObject.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
    
    private void UpdateCameraOrientation(float angle)
    {
        _orbitalFollow.HorizontalAxis.Value = angle;
    }
    
    public void GetCameraBasis(out Vector3 forward, out Vector3 right)
    {
        var yaw = this.CurrentYaw;
        var camYaw = Quaternion.Euler(0f, yaw, 0f);

        forward = camYaw * Vector3.forward;
        right = camYaw * Vector3.right;
    }
}
