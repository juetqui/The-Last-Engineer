using UnityEngine;

public class CameraTDController : MonoBehaviour
{
    [Header("Breath Sin")]
    [SerializeField] private float _breathFrequencySin = 0.75f;
    [SerializeField] private float _breathAmplitudeSin = 0.25f;
    [SerializeField] private float _lateralAmplitudeSin = 0.5f;

    [Header("Breath Cos")]
    [SerializeField] private float _breathFrequencyCos = 0.25f;
    [SerializeField] private float _breathAmplitudeCos = 0.125f;
    [SerializeField] private float _lateralAmplitudeCos = 0.25f;

    private Vector3 _basePos = default;

    private void Awake()
    {
        _basePos = transform.position;
    }

    void LateUpdate()
    {
        ApplyBreathEffect();
    }

    private void ApplyBreathEffect()
    {
        float breathSinY = Mathf.Sin(Time.time * _breathFrequencySin) * _breathAmplitudeSin;
        float breathSinX = Mathf.Sin(Time.time * _breathFrequencySin * 0.5f) * _lateralAmplitudeSin;


        float breathCosY = Mathf.Cos(Time.time * _breathFrequencyCos) * _breathAmplitudeCos;
        float breathCosX = Mathf.Cos(Time.time * _breathFrequencyCos * 0.5f) * _lateralAmplitudeCos;

        float breathSinXY = breathSinX + breathSinY;
        float breathCosXY = breathCosX + breathCosY;

        float breathX = Mathf.Lerp(breathSinX + breathSinY, breathSinXY + breathCosXY, 0.5f);
        float breathY = Mathf.Lerp(breathCosX + breathCosY, breathSinXY + breathCosXY, 0.5f);

        transform.position = _basePos + new Vector3(breathX, breathY, 0f);
    }
}
