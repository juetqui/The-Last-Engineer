using System.Collections;
using UnityEngine;

public class EmissiveListener : MonoBehaviour
{
    [SerializeField] private Connection _connection;
    [SerializeField] private float _transitionDuration = 0.25f;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _enabledColor;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _disabledColor;
    
    [SerializeField] private float _lerpTime = 1f;

    private Renderer _renderer = default;
    private Coroutine _currentCoroutine = null;

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
        if (connected)
        {
            _targetColor = _enabledColor;
            //if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            //_currentCoroutine = StartCoroutine(ChangeEmissive(_enabledColor));
        }
        else
        {
            _targetColor = _disabledColor;
            //if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            //_currentCoroutine = StartCoroutine(ChangeEmissive(_disabledColor));
        }

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

    private IEnumerator ChangeEmissive(Color targetColor)
    {
        float elapsed = 0f;

        Color vfxColor = _renderer.material.GetColor("_EmissiveColor");

        while (elapsed < _transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _transitionDuration;

            Color vfxNewColor = Color.Lerp(vfxColor, targetColor, t);

            _renderer.material.SetColor("_EmissiveColor", vfxNewColor);

            yield return null;
        }

        _renderer.material.SetColor("_EmissiveColor", targetColor);
        _currentCoroutine = null;
    }
}
