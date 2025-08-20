using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalController2 : MonoBehaviour
{
    public Material MyMaterial;
    void Awake()
    {
        GetComponent<DecalProjector>().material = new Material(MyMaterial);  
    }
}
