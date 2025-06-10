using System;
using UnityEngine;

public class BigPressurePlate : MonoBehaviour
{
    private PlatesGlitcheable _glitcheable = null;
    private Renderer _renderer = default;
    private Vector3 _originalPos = Vector3.zero;
    private bool _pressed = false;

    public Action<bool> OnPressed = delegate { };

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalPos = transform.position;
        UpdateFeedback(_pressed);
    }

    private void Start()
    {
        PlatesActivator.Instance.OnActivatePlates += ActivatePlate;
    }

    private void ActivatePlate(bool activate)
    {
        if (!activate || _glitcheable != null && !_glitcheable.IsStopped) return;

        OnPressed?.Invoke(_pressed);
    }

    private void UpdateFeedback(bool pressed, PlateType color = PlateType.None)
    {
        Color newColor = Color.white;

        if (color == PlateType.Green)
            newColor = Color.green;
        else if (color == PlateType.Blue)
            newColor = Color.blue;
        else if(color == PlateType.Purple)
            newColor = Color.magenta;

        _renderer.material.color = pressed ? newColor : Color.yellow;

        Vector3 pressedPos = new Vector3(_originalPos.x, _originalPos.y - 0.1f, _originalPos.z);

        Vector3 newPos = pressed ? pressedPos : _originalPos;
        transform.position = newPos;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.TryGetComponent(out PlatesGlitcheable glitcheable))
        {
            _pressed = true;
            UpdateFeedback(true, glitcheable.colorType);
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.TryGetComponent(out PlatesGlitcheable glitcheable))
        {
            _glitcheable = glitcheable;
            _pressed = true;
            UpdateFeedback(true, glitcheable.colorType);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out PlatesGlitcheable glitcheable))
        {
            if (glitcheable == _glitcheable) _glitcheable = null;

            _pressed = false;
            UpdateFeedback(_pressed);
        }
    }
}
