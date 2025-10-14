using UnityEngine;
using Cinemachine;
using System.Collections;

public class FollowController : MonoBehaviour
{
    private CinemachineFreeLook _camera = default;
    private PlayerController _player = default;

    void Start()
    {
        _camera = GetComponent<CinemachineFreeLook>();
        _player = PlayerController.Instance;

        _player.OnDied += StopFollowing;
        _player.OnRespawned += StartFollowing;
    }

    private void StopFollowing()
    {
        _camera.Follow = null;
        StartCoroutine(StopLooking());
    }

    private void StartFollowing()
    {
        _camera.Follow = _player.transform;
        _camera.LookAt = _player.transform;
    }

    private IEnumerator StopLooking()
    {
        yield return new WaitForSeconds(1f);
        _camera.LookAt = null;
    }
}
