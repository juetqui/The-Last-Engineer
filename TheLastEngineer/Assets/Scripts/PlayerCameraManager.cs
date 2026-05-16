using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private Collider _trigger;
    [SerializeField] private LeanTweenType _tweenType;
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
            LeanTween.value(gameObject, _startHeight, _targetHeight, _duration)
                .setEase(_tweenType)
                .setOnUpdate((float value) =>
                {
                    var orbits = _orbitalFollow.Orbits;
                    orbits.Center.Height = value;
                    _orbitalFollow.Orbits = orbits;
                })
                .setOnComplete(() => _trigger.enabled = false);
        }
    }
}
