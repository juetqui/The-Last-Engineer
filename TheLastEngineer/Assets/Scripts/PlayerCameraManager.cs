using Cinemachine;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _camera;
    [SerializeField] private Collider _trigger;
    [SerializeField] private LeanTweenType _tweenType;
    [SerializeField] private float _duration;
    
    private IMovablePassenger _player = null;
    private Vector3 _lastPosition = Vector3.zero;
    private bool _hasPlayerOn = false;
    private float _startHeight = 0f, _targetHeight = 30f;

    private void Start()
    {
        _startHeight = _camera.m_Orbits[1].m_Height;
    }

    private void LateUpdate()
    {
        if (!_hasPlayerOn || _player == null)
            return;

        Vector3 displacement = transform.position - _lastPosition;

        if (displacement.sqrMagnitude > 0.0001f)
            _player.OnPlatformMoving(displacement);

        _lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            _player = player;
            _hasPlayerOn = true;
            _lastPosition = transform.position;

            LeanTween.value(gameObject, _targetHeight, _startHeight, _duration)
                .setEase(_tweenType)
                .setOnUpdate((float value) =>
                {
                    _camera.m_Orbits[1].m_Height = value;
                });
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            LeanTween.value(gameObject, _startHeight, _targetHeight, _duration)
                .setEase(_tweenType)
                .setOnUpdate((float value) =>
                {
                    _camera.m_Orbits[1].m_Height = value;
                });
        }
    }
}
