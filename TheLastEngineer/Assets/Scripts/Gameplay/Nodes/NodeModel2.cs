using UnityEngine;

public sealed class NodeModel2
{
    public NodeType NodeType { get; private set; }
    public bool IsChild { get; private set; }
    public bool IsConnected { get; private set; }

    // Config de “movimiento” lógico
    public float MinY { get; }
    public float MaxY { get; }
    public float MoveSpeed { get; }
    public float RotSpeed { get; }

    public Vector3 ResetWorldPos { get; private set; }
    public float InitialY { get; private set; }

    public NodeModel2(NodeType nodeType, float minY, float maxY, float moveSpeed, float rotSpeed, Vector3 initialPos)
    {
        NodeType = nodeType;
        MinY = minY; MaxY = maxY; MoveSpeed = moveSpeed; RotSpeed = rotSpeed;
        ResetWorldPos = initialPos;
        InitialY = initialPos.y;
    }

    public void SetAsChild(bool isChild) => IsChild = isChild;
    public void SetConnected(bool connected) => IsConnected = connected;

    public void ToggleType()
    {
        NodeType = NodeType == NodeType.Default ? NodeType.Corrupted : NodeType.Default;
    }

    // Devuelve la Y target según el tiempo
    public float EvaluateY(float time)
    {
        float t = (Mathf.Sin(time * MoveSpeed) + 1f) * 0.5f;
        float offset = Mathf.Lerp(MinY, MaxY, t);
        return InitialY + offset;
    }

    public void UpdateInitialFrom(Vector3 newWorldPos)
    {
        ResetWorldPos = newWorldPos;
        InitialY = newWorldPos.y;
    }
}
