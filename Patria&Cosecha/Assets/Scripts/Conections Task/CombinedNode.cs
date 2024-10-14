using UnityEngine;

public class CombinedNode : ElectricityNode
{
    private Renderer _renderer;

    void Start()
    {
        StartUpNode();
    }

    private void Update()
    {

    }

    public void Attach(Transform player)
    {
        transform.SetParent(player, false);
        transform.localScale = Vector3.one;
        transform.localPosition = new Vector3(0, 0, 1.5f);
    }

    private void StartUpNode()
    {
        _renderer = GetComponent<Renderer>();

        if (this.NodeType == NodeType.CubeCapsule)
        {
            //_renderer.material.SetColor("CubeColor", FF00FF);
        }
    }
}
