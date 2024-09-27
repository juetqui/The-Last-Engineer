using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = default, _dashSpeed = default, _rotSpeed = default, _taskInteractionDistance = 2f;
    [SerializeField] private ElectricityNode _cubeNode = default, _sphereNode = default, _capsuleNode = default;

    private float _horizontalInput = default, _verticalInput = default;
    private bool _isInGrabArea = false, _isInPlaceArea = false;
    private Vector3 _moveDir = default;
    private NodeType _currentNode = default;
    private Rigidbody _rb = default;
    private ElectricityNode _nodeToChange = default;
    private ConnectionNode _nodeToConnect = default;

    public float TaskInteractionDistance { get { return _taskInteractionDistance; } }
    public NodeType CurrentNode { get { return _currentNode; } }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentNode = NodeType.None;

        _cubeNode.gameObject.SetActive(false);
        _sphereNode.gameObject.SetActive(false);
        _capsuleNode.gameObject.SetActive(false);
        CheckCurrentNode();
    }

    private void Update()
    {
        if (_isInGrabArea && _nodeToChange != null) ChangeNode(_nodeToChange.NodeType);
        if (_isInPlaceArea && _nodeToConnect != null) PlaceNode();

        ResetLevel();
    }

    private void FixedUpdate()
    {
        MovePlayer(GetMoveInput());
        Dash(GetMoveInput());
    }

    private Vector3 GetMoveInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _moveDir = new Vector3(_horizontalInput, 0, _verticalInput);

        return _moveDir;
    }

    private void MovePlayer(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0.1f) RotatePlayer(moveDir);

        Vector3 dir = moveDir.normalized * _moveSpeed;

        if (_rb.velocity.magnitude < _moveSpeed) _rb.AddForce(dir, ForceMode.VelocityChange);
    }

    private void Dash(Vector3 moveDir)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (moveDir.magnitude <= 0) moveDir = Vector3.forward;
            Debug.Log(moveDir);
            
            Vector3 dir = moveDir.normalized * _dashSpeed;
            
            _rb.AddForce(dir, ForceMode.Impulse);
        }
    }

    private void RotatePlayer(Vector3 rotDir)
    {
        Quaternion toRotation = Quaternion.LookRotation(rotDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, _rotSpeed * Time.deltaTime);
    }

    private void ChangeNode(NodeType nodeType)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _currentNode = nodeType;
            CheckCurrentNode();
        }
    }

    private void PlaceNode()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _nodeToConnect.SetNode(_currentNode);
            _currentNode = NodeType.None;
            CheckCurrentNode();
        }
    }

    private void ResetLevel()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckCurrentNode()
    {
        if (_currentNode == NodeType.Cube)
        {
            _cubeNode.gameObject.SetActive(true);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(false);
        }
        else if (_currentNode == NodeType.Sphere)
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(true);
            _capsuleNode.gameObject.SetActive(false);
        }
        else if (_currentNode == NodeType.Capsule)
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(true);
        }
        else
        {
            _cubeNode.gameObject.SetActive(false);
            _sphereNode.gameObject.SetActive(false);
            _capsuleNode.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.CompareTag("Node") && _nodeToChange == null)
        {
            ElectricityNode node = coll.gameObject.GetComponent<ElectricityNode>();

            if (node != null)
            {
                _isInGrabArea = true;
                _nodeToChange = node;
            }
        }
        else if (coll.CompareTag("Connection") && _nodeToConnect == null)
        {
            ConnectionNode node = coll.gameObject.GetComponent<ConnectionNode>();

            if (node != null)
            {
                _isInPlaceArea = true;
                _nodeToConnect = node;
            }
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.CompareTag("Node"))
        {
            _isInGrabArea = false;
            _nodeToChange = null;
        }
        else if (coll.CompareTag("Connection"))
        {
            _isInPlaceArea = false;
            _nodeToConnect = null;
        }
    }
}
