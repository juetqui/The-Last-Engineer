using UnityEngine;

public class CameraTDController : MonoBehaviour
{
    [SerializeField] private PlayerTDController _player = default;

    [Header("Breath")]
    [SerializeField] private float _breathAmplitude = 0.05f;
    [SerializeField] private float _breathFrequency = 1.5f;
    [SerializeField] private float _lateralAmplitude = 0.02f;

    [Header("Follow")]
    [SerializeField] private float _followStartDistance = 5f;
    [SerializeField] private float _followSpeed = 5f;
    [SerializeField] private float _smoothTime = 0.3f;

    private Vector3 _velocity = Vector3.zero;
    private bool _shouldFollow = false;
    private Vector3 _baseCameraPosition;

    void Start()
    {
        _baseCameraPosition = transform.position;  // Guardar la posición base de la cámara
    }

    void Update()
    {
        SmoothFollow();
        ApplyBreathEffect();

        if (_player.GetComponent<Rigidbody>().velocity.magnitude <= 0.3f) ResetCamera();
    }

    private void SmoothFollow()
    {
        float distanceTravelledX = Mathf.Abs(_player.transform.position.x);
        float distanceTravelledZ = Mathf.Abs(_player.transform.position.z);

        if (distanceTravelledX > _followStartDistance || distanceTravelledZ > _followStartDistance)
            _shouldFollow = true;

        if (_shouldFollow)
        {
            Vector3 targetPosition = new Vector3(_player.transform.position.x, _baseCameraPosition.y, _player.transform.position.z - 12f);
            _baseCameraPosition = Vector3.SmoothDamp(_baseCameraPosition, targetPosition, ref _velocity, _smoothTime, _followSpeed);
        }
    }

    private void ApplyBreathEffect()
    {
        float breathOffsetY = Mathf.Sin(Time.time * _breathFrequency) * _breathAmplitude;
        float breathOffsetX = Mathf.Sin(Time.time * _breathFrequency * 0.5f) * _lateralAmplitude;

        transform.position = _baseCameraPosition + new Vector3(breathOffsetX, breathOffsetY, 0f);
    }

    private void ResetCamera()
    {
        _shouldFollow = false;
    }
}
