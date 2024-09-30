using UnityEngine;

public class CameraTDController : MonoBehaviour
{
    public float breathAmplitude = 0.05f;
    public float breathFrequency = 1.5f;
    public float lateralAmplitude = 0.02f;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float breathOffsetY = Mathf.Sin(Time.time * breathFrequency) * breathAmplitude;
        float breathOffsetX = Mathf.Sin(Time.time * breathFrequency * 0.5f) * lateralAmplitude;
        transform.localPosition = initialPosition + new Vector3(breathOffsetX, breathOffsetY, 0f);
    }
}
