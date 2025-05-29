using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class ActiveMaterialization : MonoBehaviour
{
    public static ActiveMaterialization Instance;

    [SerializeField] private float _detectionRange = 6;

    private PlayerTDController _player = null;
    private Materializer _materializable = null, _lastChanged = null;
    private bool _isSelecting = false, _canUseAbility = false;

    public event Action<bool> OnSelectionActivated = delegate { };
    public event Action<Materializer> OnObjectSelected = delegate { };

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
            _materializable = GetNearestMaterializable();
            OnObjectSelected?.Invoke(_materializable);
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
        //InputManager.Instance.shieldInput.performed += TurnOnSelection;
        InputManager.Instance.modoDer.performed += ChangeMaterializationState;
    }

    public void OnDisableInputs()
    {
        //InputManager.Instance.shieldInput.performed -= TurnOnSelection;
        InputManager.Instance.modoDer.performed -= ChangeMaterializationState;
    }

    private void OnDestroy()
    {
        InputManager.Instance.onInputsEnabled -= OnEnableInputs;
        InputManager.Instance.onInputsDisabled -= OnDisableInputs;
    }
    #endregion

    private void CheckPlayersCurrentNode(bool hasNode, NodeType node)
    {
        if (hasNode && node == NodeType.Blue)
        {
            _canUseAbility = true;
            _isSelecting = true;
            OnSelectionActivated?.Invoke(true);
        }
        else
        {
            _canUseAbility = false;
            _isSelecting = false;
            _materializable = null;
            _lastChanged = null;
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
            _materializable = null;
            _lastChanged = null;
        }
    }

    private void ChangeMaterializationState(InputAction.CallbackContext context)
    {
        if (!_isSelecting || _materializable == null) return;

        if (_lastChanged != null && _lastChanged != _materializable)
        {
            _lastChanged.ToggleMaterialization();
        }

        _materializable.ToggleMaterialization();
        //_isSelecting = false;
        _lastChanged = _materializable;
        _materializable = null;
        //OnSelectionActivated?.Invoke(false);
    }

    private Materializer GetNearestMaterializable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange);
        Materializer closest = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Materializer materializer))
            {
                float distance = Vector3.Distance(_player.transform.position, materializer.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = materializer;
                }
            }
        }

        return closest;
    }
}

public enum SelectionType
{
    On,
    Off,
    Selected
}
