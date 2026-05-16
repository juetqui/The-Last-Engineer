using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;

public class FollowController : MonoBehaviour
{
    [SerializeField] private Transform lookAtObject;

    private CinemachineCamera _camera;
    private CinemachineOrbitalFollow _orbitalFollow;
    private PlayerController _player;
    
    private bool _canRotate = true;

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

    private void OrientateCamera(float angle, float easeTime = 0.75f, LeanTweenType easeType = LeanTweenType.easeInOutSine)
    {
        if (!_canRotate) return;

        LeanTween.value(gameObject, UpdateCameraOrientation, _orbitalFollow.HorizontalAxis.Value, angle, easeTime)
            .setOnStart(() => _canRotate = false)
            .setEase(easeType);
        
        LeanTween.value(lookAtObject.gameObject, UpdateObjectOrientation, lookAtObject.eulerAngles.y, angle, easeTime)
            .setEase(easeType)
            .setOnComplete(() => _canRotate = true);
    }

    private void UpdateObjectOrientation(float angle)
    {
        lookAtObject.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
    
    private void UpdateCameraOrientation(float angle)
    {
        _orbitalFollow.HorizontalAxis.Value = angle;
    }
}
