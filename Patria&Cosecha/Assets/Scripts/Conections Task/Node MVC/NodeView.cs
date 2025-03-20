using System.Collections;
using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Material _material = default;
    private Collider _collider = default;
    private Outline _outline = default;
    private Color _outlineColor;
    private float _emissionIntensity = default;

    private Color _baseColor = Color.white;
    private float _time = 0, _currentIntensity = 0f;
    private bool _isRunning = false, _isReseting = false;

    public bool IsReseting { get { return _isReseting; } }

    public NodeView(Renderer renderer, Material material, Collider collider, Outline outline, Color outlineColor, float emissionIntensity)
    {
        _renderer = renderer;
        _material = material;
        _collider = collider;
        _outline = outline;
        _outlineColor = outlineColor;
        _emissionIntensity = emissionIntensity;
    }

    public void OnStart()
    {
        _renderer.material = _material;
        _outline.OutlineColor = _outlineColor;
    }

    public void EnableColl(bool onOff)
    {
        _collider.enabled = onOff;
    }

    public IEnumerator ResetHability(float dashDuration, float dashCD)
    {
        _isReseting = true;

        while (_time < dashDuration)
        {
            _time += Time.deltaTime;
            _currentIntensity = Mathf.Lerp(_emissionIntensity, 0f, _time / dashDuration);
            _renderer.material.SetColor("_EmissionColor", _baseColor * _currentIntensity);
            
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        _time = 0f;
        dashCD += 0.25f;

        while (_time < dashCD)
        {
            _time += Time.deltaTime;
            _currentIntensity = Mathf.Lerp(0f, _emissionIntensity, _time / dashCD);
            _renderer.material.SetColor("_EmissionColor", _baseColor * _currentIntensity);

            yield return null;
        }

        _isReseting = false;
    }
}
