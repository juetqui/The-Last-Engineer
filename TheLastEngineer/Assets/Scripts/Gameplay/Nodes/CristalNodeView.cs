using UnityEngine;

public class CristalNodeView : MonoBehaviour
{
    [SerializeField] Renderer _effectNode;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionDefault;
    [ColorUsageAttribute(true, true)]
    [SerializeField] private Color _emissionCorrupted;

    NodeController controller;
    private Renderer _renderer;

    public void Awake()
    {
        _renderer = GetComponent<Renderer>();
        controller = GetComponentInParent<NodeController>();
        controller.OnUpdatedNodeType += ChangeColor;
    }

    void ChangeColor(GlitchState node)
    {
        if (node == GlitchState.Glitched)
        {
            _renderer.material.SetColor("_EmissiveColor", _emissionCorrupted);
            _effectNode.material.SetFloat("_isCorrupted", 1);
        }
        else
        {
            _renderer.material.SetColor("_EmissiveColor", _emissionDefault);
            _effectNode.material.SetFloat("_isCorrupted", 0);
        }
    }
}
