using UnityEngine;

public class CameraTDController : MonoBehaviour
{
    [SerializeField] private Camera _camera;

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

    private void Start()
    {
        Adjust();
    }

    private void LateUpdate()
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

    public void Adjust()
    {
        Rect rect = _camera.rect;

        float targetaspect = 16.0f / 9.0f;

        float windowaspect = (float)Screen.width / (float)Screen.height;

        float scaleheight = windowaspect / targetaspect;

        if (scaleheight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            _camera.rect = rect;
        }
        else
        {
            float scalewidth = 1.0f / scaleheight;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            _camera.rect = rect;
        }
    }
}
