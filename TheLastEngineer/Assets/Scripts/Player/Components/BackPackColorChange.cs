using UnityEngine;

public class BackPackColorChange : MonoBehaviour
{
    private Renderer _renderer = default;
    private Color _targetColor = default;

    private Material _defaultMat;
    private Material _intangibleMat;
    
    private float _timer = 0f, _lerpTime = 1f;
    private float _targetAlpha = 0f;
    private bool _changeColor = false;
    private bool _changeAlpha = false;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        
        _defaultMat = _renderer.materials[0];
        _intangibleMat = _renderer.materials[1];
        
        _defaultMat.SetColor("_EmissiveColor", Color.black);

        PlayerNodeHandler.Instance.OnNodeGrabbed += CheckNode;
    }

    private void OnDestroy()
    {
        if (PlayerNodeHandler.Instance != null) PlayerNodeHandler.Instance.OnNodeGrabbed -= CheckNode;
    }

    private void Update()
    {
        if (_changeColor) ChangeColor();
        if (_changeAlpha) ChangeAlpha();
    }

    private void CheckNode(bool hasNode, GlitchState level)
    {
        _targetColor = hasNode ? GlitchPalette.Default.EmissionFor(level) : Color.black;
        _targetAlpha = level == GlitchState.Intangible ? 1f : 0f;

        _changeColor = true;
        _changeAlpha = true;
    }

    private void ChangeAlpha()
    {
        var currentAlpha = _intangibleMat.GetFloat("_Alpha");
        var newAlpha = Mathf.Lerp(currentAlpha, _targetAlpha, _timer);
        
        _intangibleMat.SetFloat("_Alpha", newAlpha);
    }

    private void ChangeColor()
    {
        _timer += Time.deltaTime;
        
        Color currentColor = _defaultMat.GetColor("_EmissiveColor");
        Color newColor = Color.Lerp(currentColor, _targetColor, _timer);
        
        _defaultMat.SetColor("_EmissiveColor", newColor);

        if (_timer >= _lerpTime)
        {
            _changeColor = false;
            _timer = 0f;
        }
    }
}
