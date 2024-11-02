using UnityEngine;

public class BaseCamera : MonoBehaviour
{
    [SerializeField] protected Camera _camera;

    [Header("Breath Sin")]
    [SerializeField] protected float _breathFrequencySin = 0.75f;
    [SerializeField] protected float _breathAmplitudeSin = 0.25f;
    [SerializeField] protected float _lateralAmplitudeSin = 0.5f;

    [Header("Breath Cos")]
    [SerializeField] protected float _breathFrequencyCos = 0.25f;
    [SerializeField] protected float _breathAmplitudeCos = 0.125f;
    [SerializeField] protected float _lateralAmplitudeCos = 0.25f;

    protected Vector3 _basePos = default;

    protected void ApplyBreathEffect()
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

    protected void Adjust()
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
