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
    private bool _isActivated = false, _canUseAbility = false;

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
        if (_isActivated)
        {
            _materializable = GetNearestMaterializable();

            if (_materializable != null)
            {
                OnObjectSelected?.Invoke(_materializable);
            }
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
        }
        else
        {
            _canUseAbility = false;
            _isActivated = false;
            
            if (_materializable != null)
            {
                _materializable = null;
                OnSelectionActivated?.Invoke(false);
            }
        }
    }

    public void TurnOnSelection(InputAction.CallbackContext context)
    {
        if (!_canUseAbility) return;

        _isActivated = !_isActivated;

        if (!_isActivated)
        {
            OnSelectionActivated?.Invoke(false);
            _materializable = null;
            return;
        }

        OnSelectionActivated?.Invoke(true);
    }

    public void ChangeMaterializationState(InputAction.CallbackContext context)
    {
        if (_materializable == null) return;

        _materializable.ToggleMaterialization();
        _materializable = null;
        _isActivated = false;

        OnSelectionActivated?.Invoke(false);
    }

    private Materializer GetNearestMaterializable()
    {
        _materializable = null;

        var materializers = new List<Materializer>();
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _detectionRange);

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Materializer materializer))
            {
                materializers.Add(materializer);
            }
        }

        return materializers.OrderBy(m => Vector3.Distance(_player.transform.position, m.transform.position))
                           .FirstOrDefault();
    }
}

public enum SelectionType
{
    Selecting,
    Selected,
    Canceled
}
