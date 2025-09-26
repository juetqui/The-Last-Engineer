using System.Collections;
using UnityEngine;

public class EmissiveListener : MonoBehaviour
{
    [SerializeField] private Connection _connection;
    [SerializeField] private float _transitionDuration = 0.25f;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _enabledColor;
    [SerializeField] private Color _disabledColor;

    private Renderer _renderer = default;
    [SerializeField] NodeType _requiredNode = NodeType.Default;
    private Coroutine _currentCoroutine = null;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _connection.OnNodeConnected += CheckConnectedNode;

        CheckConnectedNode(_requiredNode, _connection.StartsConnected);
    }

    private void CheckConnectedNode(NodeType nodeType, bool connected)
    {
        if (connected && nodeType == _requiredNode)
        {
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(ChangeEmissive(_enabledColor));
        }
        else
        {
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(ChangeEmissive(_disabledColor));
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
