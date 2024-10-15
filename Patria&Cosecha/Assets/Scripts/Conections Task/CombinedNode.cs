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

    public void Attach(Transform newParent, Vector3 newPos)
    {
        transform.SetParent(newParent, false);   
        transform.localScale = Vector3.one;
        transform.localPosition = newPos;
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
