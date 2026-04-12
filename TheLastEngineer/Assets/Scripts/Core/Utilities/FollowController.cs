using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;

public class FollowController : MonoBehaviour
{
    public static FollowController Instance;
    
    [SerializeField] private Transform lookAtObject;

    private CinemachineFreeLook _camera;
    private PlayerController _player;
    
    private bool _canRotate = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        _camera = GetComponent<CinemachineFreeLook>();
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
        OrientateCamera(_camera.m_XAxis.Value - 90f);
    }
    
    private void RotateLeft(InputAction.CallbackContext context)
    {
        OrientateCamera(_camera.m_XAxis.Value + 90f);
    }

    public void OrientateCamera(float angle, float easeTime = 0.75f, LeanTweenType easeType = LeanTweenType.easeInOutSine)
    {
        if (!_canRotate) return;
        
        LeanTween.value(gameObject, UpdateCameraOrientation, _camera.m_XAxis.Value, angle, easeTime)
            .setEase(easeType)
            .setOnComplete(() => _canRotate = true);
    }

    private void UpdateCameraOrientation(float angle)
    {
        _canRotate = false;
        _camera.m_XAxis.Value = angle;
    }

    // private float WrapAngle(float angle)
    // {
    //     return Mathf.Repeat(angle + 180f, 360f) - 180f;
    // }
}
