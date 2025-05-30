using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActiveObjectMove : MonoBehaviour
{
    public static ActiveObjectMove Instance;

    [SerializeField] private float _detectionRange = 6;

    private PlayerTDController _player = null;
    private MoveObject _moveObject = null, _lastMoved = null;
    private bool _isSelecting = false, _canUseAbility = false;

    public event Action<bool> OnSelectionActivated = delegate { };
    public event Action<MoveObject> OnObjectSelected = delegate { };

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        StartInputs();

        _player = PlayerTDController.Instance;
        _player.OnNodeGrabed += CheckPlayersCurrentNode;
    }

    private void Update()
    {
        if (_isSelecting)
        {
            _moveObject = GetNearestMoveObject();
            OnObjectSelected?.Invoke(_moveObject);
        }
    }

    #region INPUTS MANAGEMENT
    public void StartInputs()
    {
        InputManager.Instance.onInputsEnabled += OnEnableInputs;
        InputManager.Instance.onInputsDisabled += OnDisableInputs;

        if (InputManager.Instance.playerInputs.Player.enabled) OnEnableInputs();
    }

    public void OnEnableInputs()
    {
        InputManager.Instance.modoDer.performed += ChangeObjectTarget;
    }

    public void OnDisableInputs()
    {
        InputManager.Instance.modoDer.performed -= ChangeObjectTarget;
    }

    private void OnDestroy()
    {
        InputManager.Instance.onInputsEnabled -= OnEnableInputs;
        InputManager.Instance.onInputsDisabled -= OnDisableInputs;
    }
    #endregion

    private void CheckPlayersCurrentNode(bool hasNode, NodeType node)
    {
        if (hasNode && node == NodeType.Green)
        {
            _canUseAbility = true;
            _isSelecting = true;
            OnSelectionActivated?.Invoke(true);
        }
        else
        {
            _canUseAbility = false;
            _isSelecting = false;
            _moveObject = null;
            _lastMoved = null;
            OnSelectionActivated?.Invoke(false);
        }
    }

    private void TurnOnSelection(InputAction.CallbackContext context)
    {
        if (!_canUseAbility) return;

        _isSelecting = !_isSelecting;
        OnSelectionActivated?.Invoke(_isSelecting);

        if (!_isSelecting)
        {
            _moveObject = null;
            _lastMoved = null;
        }
    }

    private void ChangeObjectTarget(InputAction.CallbackContext context)
    {
        if (!_isSelecting || _moveObject == null) return;

        if (_lastMoved != null && _lastMoved != _moveObject)
        {
            _lastMoved.TogglePos();
        }

        _moveObject.TogglePos();
        _lastMoved = _moveObject;
        _moveObject = null;
    }

    private MoveObject GetNearestMoveObject()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange);
        MoveObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out MoveObject moveObject))
            {
                float distance = Vector3.Distance(_player.transform.position, moveObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = moveObject;
                }
            }
        }

        return closest;
    }
}
