using System;
using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Material _commonMat;
    [SerializeField] private Material _successMat;
    [SerializeField] private float _pressedCD;

    private MeshRenderer _renderer;
    
    public Action<bool> OnPlayerPressed;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = _commonMat;
    }

    private IEnumerator PressedCD()
    {
        yield return new WaitForSeconds(_pressedCD);
        _renderer.material = _commonMat;
        OnPlayerPressed(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerTDController>())
        {
            OnPlayerPressed(true);
            _renderer.material = _successMat;
            StartCoroutine(PressedCD());
        }
    }
}
