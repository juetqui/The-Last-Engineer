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
    [SerializeField] AudioClip _walkClip = default;
    [SerializeField] AudioClip _grabClip = default;

    private float _horizontalInput = default, _verticalInput = default;
    private bool _canDash = false, _isDashing = false, _isInPlaceArea = false;
    
    private Vector3 _moveDir = default;
    
    private Rigidbody _rb = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _playerModel = new PlayerTDModel(_rb, transform, _groundMask, _moveSpeed, _rotSpeed, _dashSpeed, _dashDrag, _dashCooldown);
        _playerView = new PlayerTDView(_source, _walkClip, _grabClip);
    }

    private void Update()
    {
        CheckAbility();
        _playerView.WalkSound(GetMovement());

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

    private void CheckInteraction()
    {
        Debug.Log("Node: " + _node);
        Debug.Log("Connection: " + _connectionNode);
        if (_node != null)
        {
            if (_connectionNode != null && _isInPlaceArea) PlaceNode();
            else ChangeNode();
        }
    }

    private void ChangeNode()
    {
        Vector3 attachPos = new Vector3(0, 0, 1.2f);
        _node.Attach(this, attachPos);
        _playerView.GrabNode();
    }

    private void PlaceNode()
    {
        _connectionNode.SetNode(_node);
        _node = null;
        _connectionNode = null;
    }

    private void CombineNode()
    {
        _node = null;
    }

    private void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckAbility()
    {
        if (_node != null && _node.NodeType == NodeType.Dash) _canDash = true;
        else _canDash = false;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.GetComponent<ElectricityNode>() != null)
        {
            _node = coll.GetComponent<ElectricityNode>();
            _connectionNode = null;
            _isInPlaceArea = false;
        }
        else if (coll.GetComponent<ConnectionNode>() != null)
        {
            _connectionNode = coll.GetComponent<ConnectionNode>();
            _isInPlaceArea = true;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.GetComponent<ElectricityNode>() != null) _node = null;
        else if (coll.GetComponent<ConnectionNode>() != null) _connectionNode = null;
    }
    
    private IEnumerator DashCooldown()
    {
        _isDashing = true;
        yield return new WaitForSeconds(_dashCooldown);
        _playerModel.EndDash();
        _isDashing = false;
    }
}
