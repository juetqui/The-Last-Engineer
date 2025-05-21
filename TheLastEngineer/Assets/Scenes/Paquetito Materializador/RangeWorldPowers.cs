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
    private List<Materializer> _materializables = default;
    private Materializer _materializable = default;
    
    private int _selectedItem = 0;
    private bool _isActivated = false;
    
    public event Action MaterializeReset;
    public event Action<Transform> MaterializeSelection;
    
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        
        Instance = this;
        _materializables = new List<Materializer>();
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
        InputManager.Instance.shieldInput.performed += OnModoDetective;
        //InputManager.Instance.shieldInput.canceled += OffModoDetective;
        //InputManager.Instance.modoIzq.performed += context => ChangeSelection(-1);
        //InputManager.Instance.modoDer.performed += context => ChangeSelection(1);
        InputManager.Instance.modoDer.performed += ChangeMaterial;
    }
    public void OnDisableInputs()
    {
        InputManager.Instance.shieldInput.performed -= OnModoDetective;
        //InputManager.Instance.shieldInput.canceled -= OffModoDetective;
        //InputManager.Instance.modoIzq.performed -= context => ChangeSelection(-1);
        //InputManager.Instance.modoDer.performed -= context => ChangeSelection(1);
        InputManager.Instance.modoDer.performed -= ChangeMaterial;
    }
    private void OnDestroy()
    {
        InputManager.Instance.onInputsEnabled -= OnEnableInputs;
        InputManager.Instance.onInputsDisabled -= OnDisableInputs;
    }

    private void Update()
    {
        if (_isActivated)
        {
            _materializable = GetNearestMaterializable();
            _materializable.IsSelected = true;
        }
    }

    public void ChangeMaterial(InputAction.CallbackContext context)
    {
        if (_materializable == null) return;

        _materializable.ArtificialMaterialize();
        _materializable.IsAble = false;
        _materializable.IsSelected = false;
        _materializable = null;
        _isActivated = false;
        MaterializeReset?.Invoke();
    }

    public void OffModoDetective(InputAction.CallbackContext context)
    {
        //MaterializeReset?.Invoke();

        //foreach (var item in _materializables)
        //{
        //    if (item.IsSelected)
        //    {
        //        item.ArtificialMaterialize();
        //    }

        //    item.IsSelected = false;
        //    item.IsAble = false;
        //}

        //_isActivated = false;
        //_materializables.Clear();
        //_selectedItem = 0;

        //MaterializeSelection?.Invoke(_player.transform);
    }

    public void OnModoDetective(InputAction.CallbackContext context)
    {
        if (_player._currentNodeType != NodeType.Blue) return;

        _isActivated = true;
        //_selectedItem = 0;
        //_materializables.Clear();
        MaterializeReset?.Invoke();

        //_materializables = GetMaterializablesInRange();

        //foreach (var materializer in _materializables)
        //{
        //    materializer.IsAble = true;
        //}

        //if (_materializables.Count > 0)
        //{
        //    _materializables[0].IsSelected = true;
        //    MaterializeSelection?.Invoke(_materializables[0].transform);
        //}
    }

    private Materializer GetNearestMaterializable()
    {
        if (_materializable != null)
        {
            _materializable.IsAble = false;
            _materializable.IsSelected = false;
            _materializable = null;
        }

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

    private List<Materializer> GetMaterializablesInRange()
    {
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
                           .ToList();
    }

    private void ChangeSelection(int direction)
    {
        if (_materializables.Count == 0 || !_isActivated) return;

        _materializables[_selectedItem].IsSelected = false;
        _selectedItem = (_selectedItem + direction + _materializables.Count) % _materializables.Count;
        _materializables[_selectedItem].IsSelected = true;
        MaterializeSelection?.Invoke(_materializables[_selectedItem].transform);
    }
}
