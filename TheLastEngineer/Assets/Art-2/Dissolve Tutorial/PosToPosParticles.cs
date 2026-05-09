using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class PosToPosParticles : MonoBehaviour
{
    Transform targetTransform;
    public VisualEffect VFXGraph;
    public float ParticleSpeed;
    public Texture ParticleTexture;
    Vector3 particleDirection;
    Vector3 ParticleKillerPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Update()
    {
        if (targetTransform!=null)
        {
            transform.LookAt(targetTransform);
        }
    }
    public void SetTarget(Transform target)
    {
        targetTransform = target;
        particleDirection = (targetTransform.position - transform.position).normalized;
        VFXGraph.SetVector3("ParticlesSpeed", particleDirection * ParticleSpeed);
        ParticleKillerPos = targetTransform.position;
    }
    public void StartParticle(Transform target)
    {
        SetTarget(target);
        VFXGraph.Play();
    }
    public void StopParticle()
    {
        VFXGraph.Stop();
    }
}
