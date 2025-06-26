using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiCorruptionTest : MonoBehaviour
{
    MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    public void onRange()
    {
        print("entro");
        meshRenderer.material.color = (Color.red);
    }

public void OutofRange()
{
        print("SAlio");

        meshRenderer.material.color = (Color.green);
}
}
