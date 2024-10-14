using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = default;
    [SerializeField] private float _rotSpeed = default;
    [SerializeField] private float _dashSpeed = default;
    [SerializeField] private LayerMask _groundMask = default;

    [Header("Nodes")]
    [SerializeField] private ElectricityNode[] _nodes = default;

    private float _horizontalInput = default, _verticalInput = default, _oldMass = default, _oldDrag = default;
    private bool _canMove = true, _isInGrabArea = false, _isInPlaceArea = false, _canDash = false, _isDashing = false;
    
    private Vector3 _moveDir = default;
    private NodeType _currentNode = default;
    
    private Rigidbody _rb = default;
    private ElectricityNode _nodeToChange = default;
    private CombinedNode _nodeToAttach = default;
    private ConnectionNode _nodeToConnect = default;
    private CombineMachine _combineMachine = default;

    public NodeType CurrentNode { get { return _currentNode; } }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _oldMass = _rb.mass;
        _oldDrag = _rb.drag;

        _currentNode = NodeType.None;
        
        TurnOffNodes();
        CheckCurrentNode();
    }

    private void Update()
    {
        if (CheckForDash()) Dash(GetMoveInput());

        CheckAbility();
        ResetLevel();
        CheckFloor();

        if (_isInGrabArea && _nodeToChange != null) ChangeNode(_nodeToChange.NodeType);
        if (_isInGrabArea && _nodeToAttach != null) AttachCombined(_nodeToAttach);
        if (_isInPlaceArea && _nodeToConnect != null || _isInPlaceArea && _nodeToAttach != null) PlaceNode();
        if (_isInPlaceArea && _combineMachine != null) CombineNode();
    }

    private void FixedUpdate()
    {
        if (_canMove) MovePlayer(GetMoveInput());
    }

    private void CheckFloor()
    {
        if (Physics.Raycast(transform.position, -transform.up, 2.5f, _groundMask)) return;
        else
        {
            Vector3 dir = new Vector3(0, -1, 0);
            _rb.AddForce(dir.normalized * 10);
            //StartCoroutine(RestartLevel());
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private Vector3 GetMoveInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _moveDir = new Vector3(_horizontalInput, 0, _verticalInput);

        return _moveDir;
    }

    private bool CheckForDash()
    {
        if (_canDash && !_isDashing && Input.GetKeyDown(KeyCode.Space)) return true;
        
        return false;
    }

    private void Dash(Vector3 moveDir)
    {
        StartCoroutine(DashCooldown());
        
        if (moveDir == Vector3.zero) return;

        moveDir = new Vector3(moveDir.x, moveDir.y + 0.2f, moveDir.z);

        Vector3 dir = moveDir.normalized * _dashSpeed;

        _rb.AddForce(dir, ForceMode.Impulse);
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

    private void AttachCombined(CombinedNode combined)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            combined.Attach(transform);
            _currentNode = combined.NodeType;
        }
    }

    private void PlaceNode()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_currentNode == NodeType.CubeCapsule)
            {
                Debug.Log(_nodeToAttach);
                _nodeToConnect.SetCombined(_nodeToAttach);
            }
            else _nodeToConnect.SetNode(_currentNode);


            _nodeToConnect = null;
            _currentNode = NodeType.None;
            CheckCurrentNode();
        }
    }

    private void CombineNode()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _combineMachine.SetNode(_currentNode);
            _combineMachine = null;
            _currentNode = NodeType.None;
            CheckCurrentNode();
        }
    }

    private void ResetLevel()
    {
        if (Input.GetKeyDown(KeyCode.L)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckCurrentNode()
    {
        TurnOffNodes();

        foreach (var node in _nodes)
        {
            if (_currentNode == node.NodeType) node.gameObject.SetActive(true);
        }
    }

    private void TurnOffNodes()
    {
        foreach (var node in _nodes) node.gameObject.SetActive(false);
    }

    private void CheckAbility()
    {
        if (_currentNode == NodeType.CubeCapsule) _canDash = true;
        else _canDash = false;
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
        else if (coll.CompareTag("Node") && _nodeToAttach == null)
        {
            CombinedNode combinedNode = coll.gameObject.GetComponent<CombinedNode>();

            if (combinedNode != null)
            {
                _isInGrabArea = true;
                _nodeToAttach = combinedNode;
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
        else if (coll.CompareTag("Combiner") && _combineMachine == null)
        {
            CombineMachine machine = coll.GetComponent<CombineMachine>();
            
            if (machine != null)
            {
                _isInPlaceArea = true;
                _combineMachine = machine;
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
        else if (coll.CompareTag("Combiner"))
        {
            _isInPlaceArea = false;
            _combineMachine = null;
        }
    }

    private IEnumerator DashCooldown()
    {
        float oldDrag = _rb.drag;

        _isDashing = true;
        _rb.drag = oldDrag / 2;

        yield return new WaitForSeconds(2f);

        _isDashing = false;
        _rb.drag = oldDrag;
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.up * 2f);
    }
}
