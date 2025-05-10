using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Materializer : MonoBehaviour, IMaterializable
{
    [Header("<color=#FF00FF>TOGGLE TO START OBJECT (UN)-MATERIALIZED</color>")]
    [SerializeField] private bool _startsEnabled = true;
    [SerializeField] private Color _outlineColor;
    [SerializeField] private Color _outlineActiveColor;
    
    private Rigidbody _myrb = default;
    private Collider _collider = default;
    private bool _isTrigger = default;
    private MeshRenderer _renderer = default;
    private Material _enabledMat = default, _disabledMat = default, _currentMat = default;
    private Outline _outline = default;

    [SerializeField] private bool _currentEnabled = true;
    public bool IsAble;//Gabi
    public bool IsSelected; //Gabi
    public bool IsMaterializeChanged;
    public bool isMaterialized;
    public Material mySelectedMaterial; // El objeto al que le queremos cambiar el color
    public Material myAbleMaterial; // El objeto al que le queremos cambiar el color

    public static event Action<bool> OnPlayerInsideTrigger;
 
    private void Update()
    {
        if (IsSelected)
        {
            GetComponent<Renderer>().material = mySelectedMaterial;

        }
        else if (IsAble)
        {
            GetComponent<Renderer>().material = myAbleMaterial;
        }
        else
        {
            GetComponent<Renderer>().material = _currentMat;

        }
    }
    
    public void ResetSelection()
    {
        GetComponent<Renderer>().material = _enabledMat;
    }

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
        _isTrigger = _collider.isTrigger;
        _enabledMat = _renderer.material;
        _disabledMat = Resources.Load<Material>("Materials/M_Shield");
        _currentEnabled = _startsEnabled;
        _outline = gameObject.AddComponent<Outline>();
        _outline.OutlineColor = _outlineColor;
        _outline.OutlineWidth = 3;
        
        if(TryGetComponent(out Rigidbody rigidbody))
        {
            _myrb = rigidbody;
        }


        Materialize(_startsEnabled);
        
        //if (!_startsEnabled)
        //{
        //    Debug.Log(_startsEnabled);
        //    Materialize(!_startsEnabled);
        //}
        //else
        //{
        //    Materialize(_startsEnabled);
        //}

        MaterializeController.Instance.OnMaterialize += Materialize;
    }
    
    public void ArtificialMaterialize()
    {
        IsMaterializeChanged = true;
        ChangeMaterialize(!isMaterialized);

        RangeWorldPowers.Instance.MaterializeReset += DesActivate;
    }
    
    public void DesActivate()
    {
        IsMaterializeChanged = false;
        ChangeMaterialize(!isMaterialized);
        RangeWorldPowers.Instance.MaterializeReset -= DesActivate;

    }
    public void ChangeMaterialize(bool SetMaterialized)
    {
        _collider.isTrigger = !SetMaterialized;
        
        if (SetMaterialized)
        {
            _renderer.shadowCastingMode = ShadowCastingMode.On;
            _currentMat = _enabledMat;
            isMaterialized = true;
            if (_myrb)
            {
                _myrb.useGravity = true;

            }
            SetOutline(true);
        }
        else
        {
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _currentMat = _disabledMat;
            isMaterialized = false;

            if (_myrb)
            {
                _myrb.useGravity = false;
            }
            SetOutline(false);
        }
        _renderer.material = _currentMat;
    }
    
    public void Materialize(bool materialize)
    {

        //if (!_startsEnabled)
        //{
        //    materialize = !materialize;
        //}
        //if (IsMaterializeChanged)
        //{
        //    materialize = !materialize;
        //}
       // _collider.isTrigger = !materialize;
        ChangeMaterialize(materialize);
    }
    
    public void Materialize2(bool materialize)
    {

        if (!_startsEnabled)
        {
            materialize = !materialize;
        }

        print(materialize);
        _collider.isTrigger = !materialize;
        if (materialize)

        {
            _renderer.shadowCastingMode = ShadowCastingMode.On;
            _currentMat = _enabledMat;
            if (_myrb)
            {
                _myrb.useGravity = true;

            }
            SetOutline(true);
        }
        else
        {
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _currentMat = _disabledMat;
            if (_myrb)
            {
                _myrb.useGravity = false;
            }
            SetOutline(false);
        }
        

        _renderer.material = _currentMat;
    }

    private void SetOutline(bool activate)
    {
        if (activate)
        {
            _outline.enabled = true;
        }
        else
        {
            _outline.enabled = false;
        }
    }

    public bool IsTrigger()
    {
        return _collider.isTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(_isTrigger);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerTDController player))
        {
            OnPlayerInsideTrigger?.Invoke(_isTrigger);
        }
    }
}
