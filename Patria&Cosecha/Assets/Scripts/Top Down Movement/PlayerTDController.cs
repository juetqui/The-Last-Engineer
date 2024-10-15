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
    [SerializeField] private float _dashDrag = default;
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

    private PlayerTDModel _playerModel = default;

    public NodeType CurrentNode { get { return _currentNode; } }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentNode = NodeType.None;

        _playerModel = new PlayerTDModel(_rb, transform, _groundMask, _moveSpeed, _rotSpeed, _dashSpeed, _dashDrag, _dashCooldown);
        
        TurnOffNodes();
        CheckCurrentNode();
    }

    private void Update()
    {
        CheckAbility();

        if (CheckForDash()) Dash(GetMovement());

        if (Input.GetKeyDown(KeyCode.L)) ResetLevel();

        if (_isInGrabArea && _nodeToChange != null) ChangeNode(_nodeToChange.NodeType);
        if (_isInGrabArea && _nodeToAttach != null) AttachCombined(_nodeToAttach);
        if (_isInPlaceArea && _nodeToConnect != null) PlaceNode();
        if (_isInTakeArea && _combineMachine != null) CombineNode();
    }

    private void FixedUpdate()
    {
        if (!_isDashing) _playerModel.OnUpdate(GetMovement());
    }

    private Vector3 GetMovement()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        _moveDir = new Vector3(_horizontalInput, 0, _verticalInput);

        return _moveDir;
    }

    private bool CheckForDash() => _canDash && !_isDashing && Input.GetKeyDown(KeyCode.Space);

    private void Dash(Vector3 moveDir)
    {
        _playerModel.Dash(moveDir);
        StartCoroutine(DashCooldown());
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
            if (_currentNode == node.NodeType) node.gameObject.SetActive(true);
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

    private void OnDrawGizmos()
    {
        Vector3 rayDir = new Vector3(transform.localPosition.x - 0.8f, transform.localPosition.y, transform.localPosition.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayDir, -transform.up * 2f);
    }
    
    private IEnumerator DashCooldown()
    {
        _isDashing = true;
        yield return new WaitForSeconds(_dashCooldown);
        _playerModel.EndDash();
        _isDashing = false;
    }
}
