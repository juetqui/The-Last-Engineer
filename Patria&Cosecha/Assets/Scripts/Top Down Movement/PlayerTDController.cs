using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTDController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotSpeed;
    [SerializeField] private LayerMask _groundMask;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDrag;
    [SerializeField] private float _dashCooldown;

    [Header("Audio")]
    [SerializeField] AudioSource _source;
    [SerializeField] AudioClip _walkClip;
    [SerializeField] AudioClip _grabClip;

    private float _dashTimer = default;
    private bool _canDash = false, _isDashing = false, _isInPlaceArea = false;

    private bool CanDash { get { return _node != null && _node.NodeType == NodeType.Dash; }}
    
    private Rigidbody _rb = default;
    private PlayerTDModel _playerModel = default;
    private PlayerTDView _playerView = default;

    private ElectricityNode _node = default;
    private ConnectionNode _connectionNode = default;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _playerModel = new PlayerTDModel(_rb, transform, _groundMask, _moveSpeed, _rotSpeed, _dashSpeed, _dashDrag);
        _playerView = new PlayerTDView(_source, _walkClip, _grabClip);
    }

    private void Update()
    {
        _playerView.WalkSound(GetMovement());

        if (_dashTimer > 0f) _dashTimer -= Time.deltaTime;
        else if (CanDash && CheckForDash() && _dashTimer <= 0f) Dash(GetMovement());

        if (Input.GetKeyDown(KeyCode.L)) ResetLevel();
        if (Input.GetKeyDown(KeyCode.E)) CheckInteraction();
    }

    private void FixedUpdate()
    {
        if (!_isDashing) _playerModel.OnUpdate(GetMovement());
    }

    private Vector3 GetMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        return new Vector3(horizontalInput, 0, verticalInput);
    }

    private bool CheckForDash() => _canDash && !_isDashing && Input.GetKeyDown(KeyCode.Space);

    private void Dash(Vector3 moveDir)
    {
        _playerModel.Dash(moveDir);
        _dashTimer = _dashCooldown;
        _isDashing= true;
    }

    private void CheckInteraction()
    {
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
        if (_connectionNode.IsDisabled) return;

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

    private void OnTriggerEnter(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();

        if (node != null)
        {
            _node = node;
            _connectionNode = null;
            _isInPlaceArea = false;
        }
        else if (connectionNode != null)
        {
            _connectionNode = connectionNode;
            _isInPlaceArea = true;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        ElectricityNode node = coll.GetComponent<ElectricityNode>();
        ConnectionNode connectionNode = coll.GetComponent<ConnectionNode>();

        if (node != null) _node = null;
        else if (connectionNode != null) _connectionNode = null;
    }
}
