using UnityEngine;

public class BackPackColorChange : MonoBehaviour
{
    private Renderer _renderer = default;
    private Color _targetColor = default;

    // El overlay Intangible (_Alpha, slot 1) lo maneja PlayerIntangibleOverlay, compartido con el cuerpo.
    private Material _defaultMat;

    private float _timer = 0f, _lerpTime = 1f;
    private bool _changeColor = false;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        
        _defaultMat = _renderer.materials[0];
        
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
    }

    private void CheckNode(bool hasNode, GlitchState level)
    {
        _targetColor = hasNode ? GlitchPalette.Default.EmissionFor(level) : Color.black;

        _changeColor = true;
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
