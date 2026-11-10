using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;
using PrimeTween;

public class FollowController : MonoBehaviour
{
    private const float DefaultRespawnPanDuration = 1f;
    private const float LookAtReleaseDelay = 1f;

    [SerializeField] private Transform lookAtObject;
    [SerializeField] private float easeTime = 0.75f;

    [Header("Respawn pan")]
    [Tooltip("Fixed duration (seconds) of the pan from the death spot to the player at the checkpoint, whatever the distance.")]
    [SerializeField, Min(0f)] private float _respawnPanDuration = DefaultRespawnPanDuration;
    [SerializeField] private Ease _respawnPanEase = Ease.InOutSine;
    [SerializeField] private RespawnCameraPan _respawnPan;

    private Coroutine _stopLookingRoutine;

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

        // Following resumes through PanToTarget, driven by PlayerController's respawn sequence.
        _player.OnDied += StopFollowing;

        // InputManager.Instance.cameraRight.started += RotateRight;
        // InputManager.Instance.cameraLeft.started += RotateLeft;

        if (_respawnPan == null)
            Debug.LogError($"{nameof(FollowController)}: {nameof(RespawnCameraPan)} is not assigned; respawn will cut without a pan.", this);

        StartFollowing();
    }

    private void OnDestroy()
    {
        if (_player != null) _player.OnDied -= StopFollowing;
    }

    private void StopFollowing()
    {
        _camera.Follow = null;
        _stopLookingRoutine = StartCoroutine(StopLooking());
    }

    private void StartFollowing()
    {
        _camera.Follow = lookAtObject;
        _camera.LookAt = lookAtObject;
    }

    private IEnumerator StopLooking()
    {
        yield return new WaitForSeconds(LookAtReleaseDelay);
        _camera.LookAt = null;
        _stopLookingRoutine = null;
    }

    // Pans from where the camera froze on death to the player, over a fixed duration, and ends
    // framed by the composer with no leftover offset. Yield on it to wait for the pan to finish.
    public IEnumerator PanToTarget()
    {
        if (_stopLookingRoutine != null)
        {
            StopCoroutine(_stopLookingRoutine);
            _stopLookingRoutine = null;
        }

        var frozen = _camera.State;
        var frozenPosition = frozen.GetFinalPosition();
        var frozenOrientation = frozen.GetFinalOrientation();

        StartFollowing();

        // Next update ignores the stale previous frame: the composer snaps the player to the
        // screen position (CenterOnActivate) and the orbital follow skips its damping.
        _camera.PreviousStateIsValid = false;

        if (_respawnPan == null || _respawnPanDuration <= 0f) yield break;

        _respawnPan.Begin(frozenPosition, frozenOrientation);
        yield return Tween.Custom(_respawnPan, 0f, 1f, _respawnPanDuration,
            (pan, weight) => pan.Weight = weight, _respawnPanEase).ToYieldInstruction();
        _respawnPan.End();
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
