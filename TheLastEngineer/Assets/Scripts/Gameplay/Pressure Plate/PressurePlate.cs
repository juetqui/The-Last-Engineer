using System;
using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private PressuredDoor _pressureDoor;
    [SerializeField] private Material _commonMat;
    [SerializeField] private Material _successMat;
    [SerializeField] private float _pressedCD;
    
    private MeshRenderer _renderer;
    private bool _taskFinished = false;
    
    public Action<bool> OnPlayerPressed;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = _commonMat;

        _pressureDoor.OnTaskFinished += TaskFinished;
    }

    private void Update()
    {
        if (_taskFinished)
        {
            _renderer.material = _successMat;
        }
    }

    private void TaskFinished(bool taskFinished)
    {
        _taskFinished = taskFinished;
        
        if (_taskFinished)
        {
            _renderer.material = _successMat;
        }
        else
        {
            _renderer.material = _commonMat;
            OnPlayerPressed?.Invoke(false);
        }
    }

    private IEnumerator PressedCD()
    {
        _renderer.material = _successMat;
        OnPlayerPressed?.Invoke(true);

        yield return new WaitForSeconds(_pressedCD);
            
        _renderer.material = _commonMat;
        OnPlayerPressed?.Invoke(false);
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.GetComponent<PlayerController>() && !_taskFinished)
        {
            _renderer.material = _successMat;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.GetComponent<PlayerController>() && !_taskFinished)
        {
            StartCoroutine(PressedCD());
        }
    }
}
