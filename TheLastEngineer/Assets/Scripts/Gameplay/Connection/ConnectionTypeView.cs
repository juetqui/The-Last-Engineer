using UnityEngine;

public class ConnectionTypeView : MonoBehaviour
{
    [SerializeField] private LeanTweenType _tweenType = LeanTweenType.easeInOutSine;

    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionDefault;

    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionCorrupted;
    
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionOff;

    private Renderer _renderer = default;
    private Connection _connection = default;
    private Color _emissionOn = default;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _connection = GetComponentInParent<Connection>();

        _emissionOn = _connection.RequiredType == NodeType.Default ? _emissionDefault : _emissionCorrupted;

        TurnOn();
    }

    private void TurnOn()
    {
        Color currentColor = _renderer.material.GetColor("_EmissiveColor");

        LeanTween.value(gameObject, UpdateColor, currentColor, _emissionOn, 0.6f)
            .setEase(_tweenType)
            .setOnComplete(turnOff);
    }

    private void turnOff()
    {
        Color currentColor = _renderer.material.GetColor("_EmissiveColor");

        LeanTween.value(gameObject, UpdateColor, currentColor, _emissionOff, 0.6f)
            .setEase(_tweenType)
            .setOnComplete(TurnOn);
    }

    private void UpdateColor(Color c)
    {
        _renderer.material.SetColor("_EmissiveColor", c);
    }
}
