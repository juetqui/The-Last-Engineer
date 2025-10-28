using TMPro;
using UnityEngine;

public class BackPackColorChange : MonoBehaviour
{
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _defultColor;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _glitchedColor;

    private Renderer _renderer = default;
    private Color _targetColor = default;

    private float _timer = 0f, _lerpTime = 1f;
    private bool _changeColor = false;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetColor("_EmissiveColor", Color.black);

        PlayerNodeHandler.Instance.OnNodeGrabbed += CheckNode;
    }

    private void Update()
    {
        if (_changeColor) ChangeColor();
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType == NodeType.None)
            _targetColor = Color.black;
        else if (nodeType == NodeType.Default)
            _targetColor = _defultColor;
        else
            _targetColor = _glitchedColor;

        _changeColor = true;
    }

    private void ChangeColor()
    {
        _timer += Time.deltaTime;
        
        Color currentColor = _renderer.material.GetColor("_EmissiveColor");
        Color newColor = Color.Lerp(currentColor, _targetColor, _timer);
        
        _renderer.material.SetColor("_EmissiveColor", newColor);

        if (_timer >= _lerpTime)
        {
            _changeColor = false;
            _timer = 0f;
        }
    }
}
