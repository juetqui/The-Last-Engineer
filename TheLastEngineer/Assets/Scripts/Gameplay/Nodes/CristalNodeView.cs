using UnityEngine;

public class CristalNodeView : MonoBehaviour
{
    [SerializeField] Renderer _effectNode;

    NodeController controller;
    private Renderer _renderer;

    public void Awake()
    {
        _renderer = GetComponent<Renderer>();
        controller = GetComponentInParent<NodeController>();
        controller.OnUpdatedNodeType += ChangeColor;
    }

    private void OnDestroy()
    {
        if (controller != null) controller.OnUpdatedNodeType -= ChangeColor;
    }

    void ChangeColor(GlitchState level)
    {
        _renderer.material.SetColor("_EmissiveColor", GlitchPalette.Default.EmissionFor(level));

        // _isCorrupted es un booleano en S_NodeLiquidEffect: hasta que el shader tenga un tercer
        // estado, Intangible se ve como Clean y solo lo distingue el color emisivo.
        _effectNode.material.SetFloat("_isCorrupted", level == GlitchState.Glitched ? 1 : 0);
    }
}
