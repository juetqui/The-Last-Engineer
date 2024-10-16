using System.Collections;
using System.Threading;
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

    [Header("Audio")]
    [SerializeField] AudioSource _source = default;

    private float _horizontalInput = default, _verticalInput = default, _timer = default, _stepInterval = 0.25f;
    private bool _canDash = false, _isDashing = false;
    
    private Vector3 _moveDir = default;
    private NodeType _currentNode = default;
    
    private Rigidbody _rb = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;


    public NodeType CurrentNode { get { return _currentNode; } }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentNode = NodeType.None;

        _playerModel = new PlayerTDModel(_rb, transform, _groundMask, _moveSpeed, _rotSpeed, _dashSpeed, _dashDrag, _dashCooldown);
        _playerView = new PlayerTDView(_source);
    }

    private void Update()
    {
        CheckAbility();
        WalkSound();

        if (CheckForDash()) Dash(GetMovement());

        if (Input.GetKeyDown(KeyCode.L)) ResetLevel();
        if (Input.GetKeyDown(KeyCode.E)) CheckInteraction();
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

    private void WalkSound()
    {
        if (GetMovement().x != 0 || GetMovement().z != 0)
        {
            _timer += Time.deltaTime;
            
            if (_timer >= _stepInterval) _timer = _playerView.WalkSound(ref _timer);
        }
        else _timer = 0;
    }

    private void CheckInteraction()
    {
        if (_node != null) ChangeNode();
        if (_connectionNode != null) PlaceNode();
    }

    private void ChangeNode()
    {
        Vector3 attachPos = new Vector3(0, 0, 1.2f);
        _node.Attach(this, attachPos);
        _currentNode = _node.NodeType;
    }

    private void PlaceNode()
    {
        if (_currentNode != NodeType.None)
        {
            _connectionNode.SetNode(_node);
            _currentNode = NodeType.None;
            _node = null;
            _connectionNode = null;
        }
    }

    private void CombineNode()
    {
        _currentNode = NodeType.None;
    }

    private void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckAbility()
    {
        if (_currentNode == NodeType.Dash) _canDash = true;
        else _canDash = false;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.GetComponent<ElectricityNode>() != null) _node = coll.GetComponent<ElectricityNode>();
        else if (coll.GetComponent<ConnectionNode>() != null) _connectionNode = coll.GetComponent<ConnectionNode>();
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.GetComponent<ElectricityNode>() != null) _node = null;
        else if (coll.GetComponent<ConnectionNode>() != null) _connectionNode = null;
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
