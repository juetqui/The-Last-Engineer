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

        // S_NodeLiquidEffect recibe un único color en vez de elegir entre dos con un booleano: así
        // cada nivel (Intangible incluido) toma su color de la paleta sin tocar el shader.
        // ColorFor y no EmissionFor: el líquido no es emisivo y el ruido ya se suma encima.
        _effectNode.material.SetColor("_Color", GlitchPalette.Default.ColorFor(level));
    }
}
