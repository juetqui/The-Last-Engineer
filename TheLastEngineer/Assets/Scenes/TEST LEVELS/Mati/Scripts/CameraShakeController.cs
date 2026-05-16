using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    [SerializeField] private CinematicSpaceshipController spaceshipController;
    
    [Header("Camera Shake Settings")]
    [Range(0, 10)]
    [SerializeField] private int strongShakeAmplitude = 3;
    [Range(0, 10)]
    [SerializeField] private int strongShakeFrequency = 8;
    [Range(0, 10)]
    [SerializeField] private int weakShakeAmplitude = 1;
    [Range(0, 10)]
    [SerializeField] private int weakShakeFrequency = 1;

    [Header("Camera Shake Transition Settings")]
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutSine;
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

        LeanTween.value(gameObject, currentAmplitude, amplitude, tweenDuration)
            .setEase(easeType)
            .setOnUpdate(value => _shake.AmplitudeGain = value);

        LeanTween.value(gameObject, currentFrequency, frequency, tweenDuration)
            .setEase(easeType)
            .setOnUpdate(value => _shake.FrequencyGain = value);
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
