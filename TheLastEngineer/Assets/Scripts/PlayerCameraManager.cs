using Cinemachine;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook _camera;
    [SerializeField] private Collider _trigger;
    [SerializeField] private LeanTweenType _tweenType;
    [SerializeField] private float _duration;

    private float _startHeight = 0f, _targetHeight = 30f;

    private void Start()
    {
        _startHeight = _camera.m_Orbits[1].m_Height;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlayerController player))
        {
            LeanTween.value(gameObject, _startHeight, _targetHeight, _duration)
                .setEase(_tweenType)
                .setOnUpdate((float value) =>
                {
                    _camera.m_Orbits[1].m_Height = value;
                })
                .setOnComplete(() =>
                {
                    _trigger.enabled = false;
                });
        }
    }
}
