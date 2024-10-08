using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = default;
    [SerializeField] private float _rotSpeed = default;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = default;
    [SerializeField] private float _dashDuration = default;
    [SerializeField] private float _dashCooldown = default;

    [Header("Nodes")]
    [SerializeField] private ElectricityNode[] _nodes = default;
    [SerializeField] private CombinedNode[] _combinedNodes = default;

    private float _horizontalInput = default, _verticalInput = default;
    private bool _isInGrabArea = false, _isInPlaceArea = false, _availableToDash = false, _canDash = true, _isDashing = false;
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
        if (_isInGrabArea && _nodeToChange != null) ChangeNode(_nodeToChange.NodeType);
        if (_isInGrabArea && _nodeToAttach != null) AttachCombined(_nodeToAttach);
        if (_isInPlaceArea && _nodeToConnect != null) PlaceNode();
        if (_isInPlaceArea && _combineMachine != null) CombineNode();

        ResetLevel();
    }

    private void FixedUpdate()
    {
        MovePlayer(GetMoveInput());

        if (_availableToDash && Input.GetKeyDown(KeyCode.Space) && _canDash && !_isDashing)
            StartCoroutine(Dash());
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

    private void AttachCombined(CombinedNode combined)
    {
        if (Input.GetKeyDown(KeyCode.E)) combined.Attach();
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

    private void CombineNode()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _combineMachine.SetNode(_currentNode);
            _currentNode = NodeType.None;
            CheckCurrentNode();
        }
    }

    private IEnumerator Dash()
    {
        Debug.Log("Dash");

        _canDash = false;
        _isDashing = true;

        Vector3 dashDirection = _rb.velocity.normalized;

        if (dashDirection.magnitude == 0)
        {
            _isDashing = false;
            _canDash = true;
            yield break;
        }

        _rb.AddForce(dashDirection * _dashSpeed, ForceMode.VelocityChange);

        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;

        yield return new WaitForSeconds(_dashCooldown);

        _canDash = true;
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
            if (_currentNode == node.NodeType)
            {
                node.gameObject.SetActive(true);
            }
            else CheckInCombined();
        }
    }

    private void TurnOffNodes()
    {
        foreach (var node in _nodes) node.gameObject.SetActive(false);
        foreach (var combined in _combinedNodes) combined.gameObject.SetActive(false);
    }

    private void CheckInCombined()
    {
        foreach (var combined in _combinedNodes)
        {
            if (_currentNode == combined.NodeType)
            {
                combined.gameObject.SetActive(true);
            }
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
                _availableToDash = false;
            }
        }
        else if (coll.CompareTag("Node") && _nodeToAttach == null)
        {
            CombinedNode combinedNode = coll.gameObject.GetComponent<CombinedNode>();

            if (combinedNode != null)
            {
                _isInGrabArea = true;
                _nodeToAttach = combinedNode;
                //_availableToDash = true;
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
}
