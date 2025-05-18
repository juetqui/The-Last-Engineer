using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class SolvingController : MonoBehaviour
{
    public SkinnedMeshRenderer SkinnedMeshRenderer;
    public VisualEffect VFXGraph;
    // en el tutorial los materiales estaban privados pero si no no funciona 
    public Material[] skinnedMaterials;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    // Start is called before the first frame update
    void Start()
    {
        if (skinnedMaterials != null)
        {
            skinnedMaterials = SkinnedMeshRenderer.materials;
            print(skinnedMaterials[0].name);
        }
        else
        {
            print(skinnedMaterials[0].name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DissolveCo());
        }
    }
    IEnumerator DissolveCo()
    {
        if (VFXGraph != null)
        {
            VFXGraph.Play();
        }
        print("a0");

        if (skinnedMaterials.Length > 0)
        {
            float counter = 0;
            print("a");
            while (skinnedMaterials[0].GetFloat("_DissolveAmount") < 1)
            {
                print("b");

                counter += dissolveRate;
                for (int i = 0; i < skinnedMaterials.Length; i++)
                {
                    print("c");

                    skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                }
                yield return new WaitForSeconds(refreshRate);
                //decrease
            }
        }
    }
}
