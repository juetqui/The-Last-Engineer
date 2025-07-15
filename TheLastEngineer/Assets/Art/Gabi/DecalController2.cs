using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalController2 : MonoBehaviour
{
    public Material MyMaterial;
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<DecalProjector>().material = new Material(MyMaterial);  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
