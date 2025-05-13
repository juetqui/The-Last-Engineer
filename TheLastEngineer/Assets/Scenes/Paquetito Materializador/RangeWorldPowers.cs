using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class RangeWorldPowers : MonoBehaviour
{
    public static RangeWorldPowers Instance;
    
    [SerializeField] private float _teleportSphereRange;

    PlayerTDController _player = null;
    public List<Materializer> _materializables = new List<Materializer>();
    private int _selectedItem = default;
    private bool _isActivated = false;
    
    public event Action MaterializeReset;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        StartInputs();
        _player = PlayerTDController.Instance;
    }

    public void StartInputs()
    {
        InputManager.Instance.onInputsEnabled += OnEnableInputs;
        InputManager.Instance.onInputsDisabled += OnDisableInputs;
        
        if (InputManager.Instance.playerInputs.Player.enabled) OnEnableInputs();
    }
    public void OnEnableInputs()
    {
        //InputManager.Instance.dashInput.performed += Select;
        InputManager.Instance.shieldInput.performed += OnModoDetective;
        InputManager.Instance.shieldInput.canceled += OffModoDetective;
        InputManager.Instance.modoIzq.performed += ModoDetectiveIzq;
        InputManager.Instance.modoDer.performed += ModoDetectiveDer;
    }
    public void OnDisableInputs()
    {
        //InputManager.Instance.dashInput.performed -= Select;
        InputManager.Instance.shieldInput.performed -= OnModoDetective;
        InputManager.Instance.shieldInput.canceled -= OffModoDetective;
        InputManager.Instance.modoIzq.performed -= ModoDetectiveIzq;
        InputManager.Instance.modoDer.performed -= ModoDetectiveDer;
    }
    private void OnDestroy()
    {
        InputManager.Instance.onInputsEnabled -= OnEnableInputs;
        InputManager.Instance.onInputsDisabled -= OnDisableInputs;
    }

    private void Select(InputAction.CallbackContext context)
    {
        MaterializeReset?.Invoke();

        foreach (var item in _materializables)
        {
            if (item.IsSelected)
            {
                item.ArtificialMaterialize();
            }

            item.IsSelected = false;
            item.IsAble = false;
        }

        _materializables.Clear();
        _isActivated = false;
    }
    public void OffModoDetective(InputAction.CallbackContext context)
    {

        MaterializeReset?.Invoke();

        foreach (var item in _materializables)
        {
            if (item.IsSelected)
            {
                item.ArtificialMaterialize();
            }

            item.IsSelected = false;
            item.IsAble = false;
        }

        _materializables.Clear();
        _isActivated = false;
    }
    public void OnModoDetective(InputAction.CallbackContext context)
    {
        _materializables.Clear();
        MaterializeReset?.Invoke();

        if (PlayerTDController.Instance._currentNodeType == NodeType.Blue)
        {
            print(PlayerTDController.Instance._currentNodeType.ToString());

            _isActivated = true;

            List<Materializer> DetectionHits = new List<Materializer>();
            Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _teleportSphereRange);

            foreach (Collider hit in hitColliders)
            {
                if (hit.TryGetComponent(out Materializer materializer))
                {
                    DetectionHits.Add(materializer);
                }
            }

            if (_materializables.Count != 0)
            {
                foreach (var item in _materializables)
                {
                    if (!DetectionHits.Contains(item))
                    {
                        item.IsAble = false;
                        _materializables.Remove(item);

                        if (_materializables.Count - 1 <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            foreach (var item in DetectionHits)
            {
                if (_materializables.Count == 0)
                {
                    _materializables.Add(item);
                    item.IsAble = true;
                }
                else if (!_materializables.Contains(item))
                {
                    _materializables.Add(item);
                    item.IsAble = true;
                }
            }

        }
        
    }

    public void ModoDetectiveIzq(InputAction.CallbackContext context)
    {
        if (_materializables.Count <= 0 || !_isActivated) return;
        _materializables[_selectedItem].IsSelected = false;
        _selectedItem++;

        if (_selectedItem >= _materializables.Count)
        {
            _selectedItem = 0;
        }

        _materializables[_selectedItem].IsSelected = true;
    }
    
    public void ModoDetectiveDer(InputAction.CallbackContext context)
    {
        if (_materializables.Count <= 0 || !_isActivated) return;

        _materializables[_selectedItem].IsSelected = false;
        _selectedItem--;

        if (_selectedItem < 0)
        {
            _selectedItem = _materializables.Count - 1;
        }

        _materializables[_selectedItem].IsSelected = true;
    }
}
