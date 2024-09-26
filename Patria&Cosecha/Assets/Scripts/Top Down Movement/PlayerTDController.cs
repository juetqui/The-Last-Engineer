using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = default, _rotSpeed = default, _taskInteractionDistance = 2f, _taskInteractionOffset = default;
    [SerializeField] private LayerMask _taskObjectsLayer = default, _nodesLayer = default;
    [SerializeField] private ElectricityNode _cubeNode = default, _sphereNode = default, _capsuleNode = default;

    private float _horizontalInput = default, _verticalInput = default;
    private bool _isInGrabArea = false;
    private Vector3 _moveDir = default;
    private NodeType _currentNode = default;
    private Rigidbody _rb = default;
    private ElectricityNode _nodeToChange = default;

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
        if (_isInGrabArea && _nodeToChange) ChangeNode(_nodeToChange.NodeType);

        PlaceNode();
        ResetLevel();
    }

    private void FixedUpdate()
    {
        MovePlayer(GetMoveInput());
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
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, _taskInteractionDistance, _taskObjectsLayer))
        {
            ConnectionNode taskObject = hit.transform.gameObject.GetComponent<ConnectionNode>();

            if (taskObject != null && Input.GetKeyDown(KeyCode.E))
            {
                taskObject.SetNode(_currentNode);
                _currentNode = NodeType.None;
                CheckCurrentNode();
            }
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
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.CompareTag("Node"))
        {
            _isInGrabArea = false;
            _nodeToChange = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        
        Gizmos.DrawRay(rayPos, transform.forward * _taskInteractionDistance);

        //Gizmos.DrawWireSphere(rayPos, _taskInteractionDistance);
    }
}
