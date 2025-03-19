using System.Collections;
using UnityEngine;

public class NodeView
{
    private Renderer _renderer = default;
    private Material _material = default;
    private Collider _collider = default;
    private Outline _outline = default;
    private Color _outlineColor = default, _currentColor = default, _unaviable = default, _aviable = default;
    private float _resetTime = default;
    
    private float _time = 0;
    private bool _isRunning = false;

    public NodeView(Renderer renderer, Material material, Collider collider, Outline outline, Color outlineColor, Color unaviable, Color aviable, float resetTime)
    {
        _renderer = renderer;
        _material = material;
        _collider = collider;
        _outline = outline;
        _outlineColor = outlineColor;
        _unaviable = unaviable;
        _aviable = aviable;
        _resetTime = resetTime;
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

    public IEnumerator ResetHability()
    {
        float partitionedTime = _resetTime / 2;

        while (_time < partitionedTime)
        {
            _time += Time.deltaTime / _resetTime;
            _currentColor = Color.Lerp(_aviable, _unaviable, _time);
            _renderer.material.SetColor("_EmissionColor", _currentColor);
            
            yield return null;
        }

        _time = 0f;

        while (_time < partitionedTime)
        {
            _time += Time.deltaTime / _resetTime;
            _currentColor = Color.Lerp(_unaviable, _aviable, _time);
            _renderer.material.SetColor("_EmissionColor", _currentColor);

            yield return null;
        }
    }
}
