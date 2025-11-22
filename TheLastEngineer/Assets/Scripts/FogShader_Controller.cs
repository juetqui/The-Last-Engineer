using UnityEngine;

public class FogShader_Controller : MonoBehaviour
{
    Material material;
    [SerializeField] float OffsetX1, OffsetY1, OffsetX2, OffsetY2;
    [SerializeField] float RemapMin;
    [SerializeField] float RemapMax;
    [SerializeField] Color TopColor;
    [SerializeField] Color BottomColor;
    [SerializeField] float Depth, NoiseScale, NoiseScale2, HeightIntensity;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_OffsetX1", OffsetX1);
        material.SetFloat("_OffsetY1", OffsetY1);
        material.SetFloat("_OffsetX2", OffsetX2);
        material.SetFloat("_OffsetY2", OffsetY2);
        material.SetFloat("_RemapMin", RemapMin);
        material.SetFloat("_RemapMax", RemapMax);
        material.SetColor("_TopColor", TopColor);
        material.SetColor("_BottomColor", BottomColor);
        material.SetFloat("_Depth", Depth);
        material.SetFloat("_NoiseScale", NoiseScale);
        material.SetFloat("_NoiseScale2", NoiseScale2);
        material.SetFloat("_HeightIntensity", HeightIntensity);

    }
}
