using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [SerializeField] private NodeType _nodeType = default;
    [SerializeField] private Collider _collider = default;
    [SerializeField] private float _rotSpeed = default, _minY = default, _maxY = default, _moveSpeed = default;
    [SerializeField] private bool _isChildren = default;

    public NodeType NodeType { get { return _nodeType; } }
    public bool IsChildren { get { return _isChildren; } }

    private void Update()
    {
        if (!_isChildren) MoveObject();
    }

    private void MoveObject()
    {
        _collider.enabled = true;

        Vector3 currentPosition = transform.position;
        float newY = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);

        transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        transform.Rotate(0, _rotSpeed * Time.deltaTime, 0);
    }

    public void Attach(PlayerTDController player, Vector3 newPos)
    {
        _collider.enabled = false;
        Attach(player.transform, newPos, true);
    }

    public void Attach(Transform newParent, Vector3 newPos, bool parentIsPlayer = false)
    {
        if (!parentIsPlayer) _collider.enabled = true;
        
        _isChildren = true;
        
        transform.SetParent(newParent, false);
        transform.localPosition = newPos;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.LookRotation(newParent.forward);
    }
}

public enum NodeType
{
    None,
    Cube,
    Sphere,
    Capsule,
    Dash,
    CubeSphere,
    SphereCapsule
}
