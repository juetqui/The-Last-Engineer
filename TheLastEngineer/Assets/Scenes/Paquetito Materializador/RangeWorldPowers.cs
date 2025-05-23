using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Linq;

public class RangeWorldPowers : MonoBehaviour
{
    public static RangeWorldPowers Instance;

    [SerializeField] private float _detectionRange = 15f;

    private PlayerTDController _player = null;
    private Materializer _materializable = null;
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
        InputManager.Instance.shieldInput.performed += TurnOnSelection;
        InputManager.Instance.modoDer.performed += ChangeMaterializationState;
    }

    public void OnDisableInputs()
    {
        InputManager.Instance.shieldInput.performed -= TurnOnSelection;
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
            _isSelecting = false;
            OnSelectionActivated?.Invoke(false);
        }
        else
        {
            _canUseAbility = false;
            _isSelecting = false;
            _materializable = null;
            OnSelectionActivated?.Invoke(false);
        }
    }

    private void TurnOnSelection(InputAction.CallbackContext context)
    {
        if (!_canUseAbility) return;

        _isSelecting = !_isSelecting;
        OnSelectionActivated?.Invoke(_isSelecting);
        if (!_isSelecting) _materializable = null;
    }

    private void ChangeMaterializationState(InputAction.CallbackContext context)
    {
        if (!_isSelecting || _materializable == null) return;

        _materializable.ToggleMaterialization();
        _isSelecting = false;
        _materializable = null;
        OnSelectionActivated?.Invoke(false);
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
    Selecting,
    Selected,
    Canceled
}
