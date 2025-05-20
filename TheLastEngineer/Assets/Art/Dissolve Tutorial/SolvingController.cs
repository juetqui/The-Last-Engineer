using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class SolvingController : MonoBehaviour
{
    public SkinnedMeshRenderer MySkinnedMeshRenderer;
    public float MaxBound;
    public float MinBound;

    public VisualEffect VFXGraph;
    // en el tutorial los materiales estaban privados pero si no no funciona 
    public Material[] skinnedMaterials;
    [SerializeField] Transform startKillerTransform, endKillerTransform;
    float _rateQty;
    public Shader MyShader;
    public float refreshRate;
    [SerializeField] float duration;
    [SerializeField] Vector3 killerSize;
    [SerializeField] Vector3 particleSpeed;
    public int particleIncreaseRate = 20;
    public int particleInintialRate = 20;
    float _totalDissolve;
    float _killerDistance;
    // Start is called before the first frame update
    void Start()
    {
        skinnedMaterials = MySkinnedMeshRenderer.materials;
        //_totalDissolve = skinnedMaterials[0].GetFloat("_DisolveProgress");



        //if (skinnedMaterials != null)
        //{
        //    Rate = duration / refreshRate;
        //    skinnedMaterials = SkinnedMeshRenderer.materials;
        //    initialDissolve = skinnedMaterials[0].GetFloat("_DisolveProgress");
        //}
        //if (VFXGraph!=null)
        //{
        //    VFXGraph.SetVector3("StartKillerPosition", startKillerTransform.position);
        //    VFXGraph.SetVector3("EndKillerPosition", endKillerTransform.position);
        //    _rateKillerMove = (startKillerTransform.position.y - endKillerTransform.position.y)/Rate;
        //}

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
        skinnedMaterials = MySkinnedMeshRenderer.materials;

        if (skinnedMaterials != null)
        {

            foreach (var item in skinnedMaterials)
            {
                item.shader = MyShader;
                //item.SetFloat("MaxBounds", MaxBound);
                //item.SetFloat("MinBounds", MinBound);
            }
            _rateQty = duration/refreshRate;
            _totalDissolve = skinnedMaterials[0].GetFloat("_DisolveProgress");

            skinnedMaterials[0].SetFloat("_DisolveProgress", _totalDissolve);
        }
        if (VFXGraph != null)
        {
            VFXGraph.SetVector3("StartKillerPosition", startKillerTransform.position);
            VFXGraph.SetVector3("EndKillerPosition", endKillerTransform.position);
            _killerDistance = (startKillerTransform.position.y - endKillerTransform.position.y);
            VFXGraph.SetVector3("EndKillerSize", killerSize);
            VFXGraph.SetVector3("StartKillerSize", new Vector3(killerSize.x, 1 / _rateQty, killerSize.z));
            VFXGraph.SetVector3("ParticlesSpeed", particleSpeed);
            VFXGraph.SetInt("initialParticleRate", particleInintialRate);


        }
        if (skinnedMaterials.Length > 0)
        {
            Vector3 vector3 = Vector3.zero;

            float counter = 0;
            VFXGraph.Play();
            VFXGraph.SetFloat("Duration", duration);
            int particlesCount = VFXGraph.GetInt("initialParticleRate");
            while (skinnedMaterials[0].GetFloat("_DisolveProgress") > 0)
            {
                vector3 += Vector3.up * (_killerDistance / _rateQty);
                VFXGraph.SetInt("initialParticleRate", particlesCount += particleIncreaseRate);
                VFXGraph.SetVector3("StartKillerSize", (killerSize + vector3 * 1.5f));
                print(vector3);
                print(killerSize + vector3);
                counter += _totalDissolve / _rateQty;
                for (int i = 0; i < skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i].SetFloat("_DisolveProgress", _totalDissolve - counter);
                }
                yield return new WaitForSeconds(refreshRate);
                //decrease
            }
        }
        if (VFXGraph != null)
        {
            VFXGraph.Stop();
        }
    }
   

}
