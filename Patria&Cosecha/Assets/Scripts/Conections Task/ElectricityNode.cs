using UnityEngine;

public class ElectricityNode : MonoBehaviour
{
    [SerializeField] private NodeType _nodeType = default;
    [SerializeField] private float _rotSpeed = default, _minY = default, _maxY = default, _moveSpeed = default;
    [SerializeField] private bool _isChildren = default;

    public NodeType NodeType { get { return _nodeType; } }

    private void Update()
    {
        if (!_isChildren) MoveObject();
    }

    private void MoveObject()
    {
        float newY = Mathf.Lerp(_minY, _maxY, (Mathf.Sin(Time.time * _moveSpeed) + 1f) / 2f);

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(0, _rotSpeed * Time.deltaTime, 0);
    }
}
