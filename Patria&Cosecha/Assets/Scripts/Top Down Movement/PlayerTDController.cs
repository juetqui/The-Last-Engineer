using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = default;
    [SerializeField] private float _rotSpeed = default;
    [SerializeField] private LayerMask _groundMask = default;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = default;
    [SerializeField] private float _dashCooldown = default;

    [Header("Nodes")]
    [SerializeField] private ElectricityNode[] _nodes = default;

    private float _horizontalInput = default, _verticalInput = default;
    private bool _isInGrabArea = false, _isInPlaceArea = false, _isInTakeArea = false, _canDash = false, _isDashing = false;
    
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

        _currentNode = NodeType.None;
        
        TurnOffNodes();
        CheckCurrentNode();
    }

    private void Update()
    {
        CheckAbility();
        CheckFloor();

        if (CheckForDash()) Dash(GetMoveInput());
        if (Input.GetKeyDown(KeyCode.L)) ResetLevel();

        if (_isInGrabArea && _nodeToChange != null) ChangeNode(_nodeToChange.NodeType);
        if (_isInGrabArea && _nodeToAttach != null) AttachCombined(_nodeToAttach);
        if (_isInPlaceArea && _nodeToConnect != null) PlaceNode();
        if (_isInTakeArea && _combineMachine != null) CombineNode();
    }

    private void FixedUpdate()
    {
        MovePlayer(GetMoveInput());
    }

    private void CheckFloor()
    {
        Vector3 rayDir = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);

        if (Physics.Raycast(rayDir, -transform.up, 2.5f, _groundMask)) return;
        else
        {
            Vector3 dir = new Vector3(0, -1, 0);
            _rb.AddForce(dir.normalized * 20);
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
        if (moveDir == Vector3.zero) return;

        moveDir = new Vector3(moveDir.x, moveDir.y + 0.1f, moveDir.z);
        Vector3 dir = moveDir.normalized * _dashSpeed;

        _rb.AddForce(dir, ForceMode.Impulse);

        StartCoroutine(DashCooldown());
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
            combined.Attach(transform, new Vector3(0, 0, 1.5f));
            _currentNode = combined.NodeType;
        }
    }

    private void PlaceNode()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_currentNode == NodeType.CubeCapsule)
            {
                _nodeToConnect.SetCombined(_nodeToAttach);
                _nodeToAttach = null;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Void")) ResetLevel();
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
        else if (coll.CompareTag("Combined") && _nodeToAttach == null)
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
                _isInTakeArea = true;
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
            _isInTakeArea = false;
            _combineMachine = null;
        }
    }

    private IEnumerator DashCooldown()
    {
        float oldDrag = _rb.drag;

        _isDashing = true;
        _rb.drag = oldDrag / 2;

        yield return new WaitForSeconds(_dashCooldown);

        _isDashing = false;
        _rb.drag = oldDrag;
    }

    private void OnDrawGizmos()
    {
        Vector3 rayDir = new Vector3(transform.localPosition.x - 0.8f, transform.localPosition.y, transform.localPosition.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayDir, -transform.up * 2f);
    }
}
