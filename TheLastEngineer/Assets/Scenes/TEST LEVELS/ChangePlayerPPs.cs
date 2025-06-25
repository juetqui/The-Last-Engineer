using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChangePlayerPPs : MonoBehaviour
{
    [SerializeField] private UniversalRendererData rendererData;

    private bool _pp1Active = false, _pp2Active = false;

    private void Start()
    {
        SetPostProcessActiveByIndex(0, false);
        SetPostProcessActiveByIndex(1, false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _pp1Active = !_pp1Active;
            SetPostProcessActiveByIndex(0, _pp1Active);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _pp2Active = !_pp2Active;
            SetPostProcessActiveByIndex(1, _pp2Active);
        }
    }


    // Ejemplo: Activa o desactiva por �ndice (si sabes el orden en la lista)
    public void SetPostProcessActiveByIndex(int index, bool active)
    {
        rendererData.rendererFeatures[index].SetActive(active);
    }
}
