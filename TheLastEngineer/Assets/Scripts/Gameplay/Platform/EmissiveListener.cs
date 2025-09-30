using UnityEngine;

public class EmissiveListener : MonoBehaviour
{
    [SerializeField] private Connection _connection;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _enabledColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _disabledColor;
    
    [SerializeField] private float _lerpTime = 1f;

    private Renderer _renderer = default;

    private Color _targetColor;
    private float _timer = 0f;
    private bool _changeColor = false;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        CheckConnectedNode(NodeType.Default, false);

        if (_connection != null)
            _connection.OnInitialized += Initialize;
    }

    private void Initialize()
    {
        _connection.OnNodeConnected += CheckConnectedNode;
    }

    private void Update()
    {
        if (_changeColor)
            LerpColors();
    }

    private void CheckConnectedNode(NodeType nodeType, bool connected)
    {
        if (connected) _targetColor = _enabledColor;
        else _targetColor = _disabledColor;

        _changeColor = true;
    }

    private void LerpColors()
    {
        _timer += Time.deltaTime;
        
        Color currentColor = _renderer.material.GetColor("_EmissiveColor");
        Color newColor = Color.Lerp(currentColor, _targetColor, _timer);
        
        _renderer.material.SetColor("_EmissiveColor", newColor);
        
        if (_timer >= _lerpTime)
        {
            _renderer.material.SetColor("_EmissiveColor", _targetColor);
            _changeColor = false;
        }
    }
}
