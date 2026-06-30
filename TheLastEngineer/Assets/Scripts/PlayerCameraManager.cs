using Unity.Cinemachine;
using UnityEngine;
using PrimeTween;

public class PlayerCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private Collider _trigger;
    [SerializeField] private Ease _tweenType;
    [SerializeField] private float _duration;

    private CinemachineOrbitalFollow _orbitalFollow;
    private float _startHeight = 0f, _targetHeight = 30f;

    private void Awake()
    {
        _orbitalFollow = _camera.GetComponent<CinemachineOrbitalFollow>();
        _startHeight = _orbitalFollow.Orbits.Center.Height;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            Tween.Custom(gameObject, _startHeight, _targetHeight, _duration, (_, value) =>
                {
                    var orbits = _orbitalFollow.Orbits;
                    orbits.Center.Height = value;
                    _orbitalFollow.Orbits = orbits;
                }, _tweenType)
                .OnComplete(() => _trigger.enabled = false);
        }
    }

    private void OnDestroy()
    {
        Tween.StopAll(onTarget: gameObject);
    }
}
