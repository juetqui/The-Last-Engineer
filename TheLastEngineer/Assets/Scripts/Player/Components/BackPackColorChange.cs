using UnityEngine;

public class BackPackColorChange : MonoBehaviour
{
    private Renderer _renderer = default;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.SetInt("_HasCorruptedNode", 0);
        _renderer.material.SetColor("_EmissiveColor", Color.black);

        PlayerNodeHandler.Instance.OnNodeGrabbed += CheckNode;
    }

    private void CheckNode(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType == NodeType.None)
        {
            _renderer.material.SetInt("_HasCorruptedNode", 0);
            _renderer.material.SetColor("_EmissiveColor", Color.black);
        }
        else if (nodeType == NodeType.Default)
        {
            _renderer.material.SetInt("_HasCorruptedNode", 0);
            _renderer.material.SetColor("_EmissiveColor", Color.cyan);
        }
        else
        {
            _renderer.material.SetInt("_HasCorruptedNode", 1);
        }
    }
}
