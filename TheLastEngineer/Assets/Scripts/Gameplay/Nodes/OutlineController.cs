using UnityEngine;

public class OutlineController : MonoBehaviour
{
    Outline _outline;
    NodeController _nodeController;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        _nodeController = GetComponentInParent<NodeController>();
        _nodeController.OnUpdatedNodeType += ChangeOutline;
        _nodeController.OnEnableOutline += EnableOutline;
    }

    private void OnDestroy()
    {
        if (_nodeController == null) return;

        _nodeController.OnUpdatedNodeType -= ChangeOutline;
        _nodeController.OnEnableOutline -= EnableOutline;
    }

    // Los colores salen de la paleta compartida: con tres niveles, tenerlos serializados en cada
    // prefab era la forma más fácil de que el outline y el cristal quedaran desincronizados.
    private void ChangeOutline(GlitchState level)
    {
        _outline.OutlineColor = GlitchPalette.Default.EmissionFor(level);
    }

    private void EnableOutline(bool enable)
    {
        _outline.enabled = enable;
    }
}
