using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchableObject : MonoBehaviour
{
    Material myMaterial; // El objeto al que le queremos cambiar el color
    public Material mySelectedMaterial; // El objeto al que le queremos cambiar el color
    public Material myAbleMaterial; // El objeto al que le queremos cambiar el color
    public bool IsSelected;
    public bool IsAble;
    private void Start()
    {
        myMaterial = GetComponent<Renderer>().material;
    }
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
            GetComponent<Renderer>().material = myMaterial;

        }
    }
    public void ResetSelection()
    {
        GetComponent<Renderer>().material = myMaterial;

    }
}