using System;
using System.Collections;
using UnityEngine;

public class PressuredDoor : MonoBehaviour
{
    [SerializeField] private Material _commonMat;
    [SerializeField] private Material _successMat;
    [SerializeField] private PressurePlate[] _plates;
    [SerializeField] private float _restartCD;

    private Animator _animator = default;
    //private MeshRenderer _renderer = default;
    private int _pressedPlates = 0;
    private bool _taskFinished = false;

    public Action<bool> OnTaskFinished;

    void Start()
    {
        _animator = GetComponent<Animator>();
        //_renderer = GetComponent<MeshRenderer>();
        //_renderer.material = _commonMat;

        foreach (var plate in _plates)
        {
            plate.OnPlayerPressed += CheckPlates;
        }
    }

    void Update()
    {
        //if (_pressedPlates == _plates.Length && !_taskFinished)
        //{
            //StartCoroutine(RestartTask());
        //}
        //else if(_renderer.material != _commonMat && !_taskFinished)
        //{
        //    _renderer.material = _commonMat;
        //}
    }

    private void CheckPlates(bool pressed)
    {
        if (pressed)
        {
            _pressedPlates++;

            if (_pressedPlates == _plates.Length && !_taskFinished)
            {
                _taskFinished = true;
                _animator.SetBool("Open", true);
                OnTaskFinished?.Invoke(true);
                //StartCoroutine(RestartTask());
            }
        }
        else
        {
            _pressedPlates--;
            
            if (_pressedPlates < 0) _pressedPlates = 0;
        }
    }

    private IEnumerator RestartTask()
    {
        _taskFinished = true;
        _animator.SetBool("Open", true);
        OnTaskFinished?.Invoke(true);
        //_renderer.material = _successMat;

        yield return new WaitForSeconds(_restartCD);
        
        _taskFinished = false;
        _animator.SetBool("Open", false);
        OnTaskFinished?.Invoke(false);
        //_renderer.material = _commonMat;
    }
}
