using UnityEngine;
using UnityEngine.UI;

public class UpdateCrosshair : MonoBehaviour
{
    [SerializeField] Camera _camera;

    private RectTransform _crosshair = default;
    private Image _image = default;

    private void Awake()
    {
        _crosshair = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        UpdatePos(null);
    }

    void Start()
    {
        GlitchActive.Instance.OnStopableSelected += UpdatePos;
    }

    private void UpdatePos(Glitcheable glitcheable)
    {
        if (glitcheable == null)
        {
            _image.enabled = false;
            _crosshair.position = Vector3.zero;
            return;
        }

        Vector3 targetPosition = glitcheable.transform.position;
        Vector3 screenPosition = _camera.WorldToScreenPoint(targetPosition);

        if (screenPosition.z > 0)
        {
            _image.enabled = true;
            _crosshair.position = screenPosition;
        }
        else
        {
            _image.enabled = false;
            _crosshair.position = Vector3.zero;
        }
    }
}
