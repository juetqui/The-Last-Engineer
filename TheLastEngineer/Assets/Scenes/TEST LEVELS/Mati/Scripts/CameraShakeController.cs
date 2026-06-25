using Unity.Cinemachine;
using UnityEngine;
using PrimeTween;

public class CameraShakeController : MonoBehaviour
{
    [SerializeField] private CinematicSpaceshipController spaceshipController;

    [Header("Camera Shake Transition Settings")]
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private float tweenDuration = 0.5f;

    private CinemachineBasicMultiChannelPerlin _shake;

    private void Awake()
    {
        _shake = GetComponent<CinemachineBasicMultiChannelPerlin>();
        spaceshipController.OnStartAnimation += SetRumble;
    }

    private void OnDestroy()
    {
        spaceshipController.OnStartAnimation -= SetRumble;
    }

    private void SetRumble(CameraShakeType shakeType)
    {
        var currentAmplitude = _shake.AmplitudeGain;
        var currentFrequency = _shake.FrequencyGain;
        
        var targetShakeIntensity = CalculateShake(shakeType);
        var amplitude = targetShakeIntensity.x;
        var frequency = targetShakeIntensity.y;

        Tween.Custom(gameObject, currentAmplitude, amplitude, tweenDuration,
            (_, value) => _shake.AmplitudeGain = value, easeType);

        Tween.Custom(gameObject, currentFrequency, frequency, tweenDuration,
            (_, value) => _shake.FrequencyGain = value, easeType);
    }

    private static Vector2 CalculateShake(CameraShakeType  shakeType)
    {
        switch (shakeType)
        {
            case CameraShakeType.Strong:
                return new Vector2(3f, 8f);
            case CameraShakeType.Weak:
                return new Vector2(1f, 1f);
            case CameraShakeType.Stop:
            default:
                return Vector2.zero;
        }
    }
}

public enum CameraShakeType
{
    Strong,
    Weak,
    Stop
}
