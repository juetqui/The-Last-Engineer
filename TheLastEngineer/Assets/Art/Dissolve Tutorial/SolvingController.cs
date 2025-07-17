using MaskTransitions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public class SolvingController : MonoBehaviour
{
    public Action OnDissolveCompleted = delegate { };

    public SkinnedMeshRenderer MySkinnedMeshRenderer;
    public float MaxBound;
    public float MinBound;
    public float secondToDissolve;
    [SerializeField] GameObject otherParticles;
    public VisualEffect VFXGraph;
    public float ResetTimer;
    public Material[] skinnedMaterials;
    [SerializeField] Transform startKillerTransform, endKillerTransform;
    float _rateQty;
    public Shader DissolveShader;
    [SerializeField] Material myMaterials;
    public Shader DesintegrateShader;
    public float refreshRate;
    [SerializeField] float duration;
    [SerializeField] Vector3 killerSize;
    [SerializeField] Vector3 particleSpeed;
    [SerializeField] Vector2 _desintegrateVector = new Vector2(-10,26);
    public int particleIncreaseRate = 20;
    public int particleInintialRate = 20;
    float _totalDissolve;
    float _killerDistance;

    private List<Shader> _originalShaders = default;
    // Variables para almacenar los valores iniciales del VFX
    private Vector3 _initialStartKillerSize;
    private int _initialParticleRate;

    void Start()
    {
        skinnedMaterials = MySkinnedMeshRenderer.materials;
        _originalShaders = new List<Shader>();

        // Guardar valores iniciales del VFX
        _initialStartKillerSize = new Vector3(killerSize.x, 1 / (duration / refreshRate), killerSize.z);
        _initialParticleRate = particleInintialRate;

        // Inicializar shaders originales
        foreach (var material in skinnedMaterials)
        {
            _originalShaders.Add(material.shader);
        }
    }

    public void RespawnPlayer()
    {
        // Restaurar shaders originales
        skinnedMaterials = MySkinnedMeshRenderer.materials;
        for (int i = 0; i < skinnedMaterials.Length; i++)
        {
            MySkinnedMeshRenderer.materials[i].shader = _originalShaders[i];
            MySkinnedMeshRenderer.materials[i].SetFloat("_DisolveProgress", _totalDissolve);
        }

        // Reiniciar parámetros del VFX
        if (VFXGraph != null)
        {
            VFXGraph.Stop(); // Asegurarse de que el VFX esté detenido
            VFXGraph.SetVector3("StartKillerPosition", startKillerTransform.position);
            VFXGraph.SetVector3("EndKillerPosition", endKillerTransform.position);
            VFXGraph.SetVector3("EndKillerSize", killerSize);
            VFXGraph.SetVector3("StartKillerSize", _initialStartKillerSize);
            VFXGraph.SetVector3("ParticlesSpeed", particleSpeed);
            VFXGraph.SetInt("initialParticleRate", _initialParticleRate);
            VFXGraph.SetFloat("Duration", duration + secondToDissolve);
        }

        // Asegurarse de que las partículas adicionales estén desactivadas
        if (otherParticles != null)
        {
            otherParticles.SetActive(false);
        }
    }

    public void BurnShader()
    {
        skinnedMaterials = MySkinnedMeshRenderer.materials;

        StartCoroutine(DissolveCo());
    }
    public void StartDesintegrateShader() 
    {
        for (int i = 0; i < 2; i++)
        {
            MySkinnedMeshRenderer.materials[i].shader = _originalShaders[i];
            MySkinnedMeshRenderer.materials[i].SetFloat("_DisolveProgress", _totalDissolve);
            skinnedMaterials[i] = MySkinnedMeshRenderer.materials[i];
        }
        //skinnedMaterials = MySkinnedMeshRenderer.materials;
        if (skinnedMaterials != null)
        {
                for (int i = 0; i < 2; i++)
            {
                var myColor = skinnedMaterials[i].color;
                skinnedMaterials[i].shader = DesintegrateShader;
                skinnedMaterials[i].SetFloat("_ColorController", 1);
                skinnedMaterials[i].SetColor("_StartingColor", myColor);
                skinnedMaterials[i].SetVector("_MinMaxPos", _desintegrateVector);
                skinnedMaterials[i].SetFloat("_Alpha", 1);
            }
        }
    }
    public void SetDesintegrateShader(float alpha)
    {
        //StartCoroutine(DesintegrateCo());
        if (skinnedMaterials != null)
        {
            foreach (var item in skinnedMaterials)
            {
                item.SetFloat("_Alpha", alpha);
            }
        }
    }
    public void StopDesintegrateShader()
    {
        //StartCoroutine(DesintegrateCo());
        if (skinnedMaterials != null)
        {
            for (int i = 0; i < skinnedMaterials.Length; i++)
            {
                MySkinnedMeshRenderer.materials[i].shader = _originalShaders[i];
            }
        }
    }
    IEnumerator DesintegrateCo()
    {
        //if (otherParticles != null)
        //{
        //    otherParticles.SetActive(false);
        //}

        skinnedMaterials = MySkinnedMeshRenderer.materials;
        if (skinnedMaterials != null)
        {
            foreach (var item in skinnedMaterials)
            {
                var myColor = item.color;
                item.shader = DesintegrateShader;
                item.SetColor("_StartingColor", myColor);
                item.SetVector("_MinMaxPos", _desintegrateVector);
                item.SetFloat("_Alpha", 1);
            }
            _rateQty = duration / refreshRate;
            _totalDissolve = skinnedMaterials[0].GetFloat("_DisolveProgress");
            skinnedMaterials[0].SetFloat("_DisolveProgress", _totalDissolve);
        }

        //if (VFXGraph != null)
        //{
        //    VFXGraph.SetVector3("StartKillerPosition", startKillerTransform.position);
        //    VFXGraph.SetVector3("EndKillerPosition", endKillerTransform.position);
        //    _killerDistance = (startKillerTransform.position.y - endKillerTransform.position.y);
        //    VFXGraph.SetVector3("EndKillerSize", killerSize);
        //    VFXGraph.SetVector3("StartKillerSize", _initialStartKillerSize);
        //    VFXGraph.SetVector3("ParticlesSpeed", particleSpeed);
        //    VFXGraph.SetInt("initialParticleRate", _initialParticleRate);
        //    VFXGraph.SetFloat("Duration", duration + secondToDissolve);
        //    VFXGraph.Play();
        //}

        yield return new WaitForSeconds(secondToDissolve);

        if (skinnedMaterials.Length > 0)
        {
            Vector3 vector3 = Vector3.zero;
            float counter = 0;
            VFXGraph.SetFloat("Duration", duration);
            int particlesCount = _initialParticleRate;
            skinnedMaterials[0].SetFloat("_DisolveProgress", _totalDissolve);

            while (skinnedMaterials[0].GetFloat("_DisolveProgress") > 0)
            {
                vector3 += Vector3.up * (_killerDistance / _rateQty);
                VFXGraph.SetInt("initialParticleRate", particlesCount += particleIncreaseRate);
                VFXGraph.SetVector3("StartKillerSize", (killerSize + vector3 * 1.5f));
                counter += _totalDissolve / _rateQty;
                for (int i = 0; i < skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i].SetFloat("_DisolveProgress", _totalDissolve - counter);
                }
                yield return new WaitForSeconds(refreshRate);
            }
        }

        if (VFXGraph != null)
        {
            VFXGraph.Stop();
        }

        OnDissolveCompleted?.Invoke();
    }
    IEnumerator DissolveCo()
    {
        if (otherParticles != null)
        {
            otherParticles.SetActive(false);
        }

        skinnedMaterials = MySkinnedMeshRenderer.materials;
        if (skinnedMaterials != null)
        {
            foreach (var item in skinnedMaterials)
            {
                item.shader = DissolveShader;
                item.SetFloat("_MaxHeight", MaxBound);
                item.SetFloat("_MinHeight", MinBound);
            }
            //foreach (var item in skinnedMaterials)
            //{
            //    item.shader = DissolveShader;
            //    item.SetFloat("_MaxHeight", MaxBound);
            //    item.SetFloat("_MinHeight", MinBound);
            //}
            _rateQty = duration / refreshRate;
            _totalDissolve = skinnedMaterials[0].GetFloat("_DisolveProgress");
            skinnedMaterials[0].SetFloat("_DisolveProgress", _totalDissolve);
        }

        if (VFXGraph != null)
        {
            VFXGraph.SetVector3("StartKillerPosition", startKillerTransform.position);
            VFXGraph.SetVector3("EndKillerPosition", endKillerTransform.position);
            _killerDistance = (startKillerTransform.position.y - endKillerTransform.position.y);
            VFXGraph.SetVector3("EndKillerSize", killerSize);
            VFXGraph.SetVector3("StartKillerSize", _initialStartKillerSize);
            VFXGraph.SetVector3("ParticlesSpeed", particleSpeed);
            VFXGraph.SetInt("initialParticleRate", _initialParticleRate);
            VFXGraph.SetFloat("Duration", duration + secondToDissolve);
            VFXGraph.Play();
        }

        yield return new WaitForSeconds(secondToDissolve);

        if (skinnedMaterials.Length > 0)
        {
            Vector3 vector3 = Vector3.zero;
            float counter = 0;
            VFXGraph.SetFloat("Duration", duration);
            int particlesCount = _initialParticleRate;
            skinnedMaterials[0].SetFloat("_DisolveProgress", _totalDissolve);

            while (skinnedMaterials[0].GetFloat("_DisolveProgress") > 0)
            {
                vector3 += Vector3.up * (_killerDistance / _rateQty);
                VFXGraph.SetInt("initialParticleRate", particlesCount += particleIncreaseRate);
                VFXGraph.SetVector3("StartKillerSize", (killerSize + vector3 * 1.5f));
                counter += _totalDissolve / _rateQty;
                //for (int i = 0; i < skinnedMaterials.Length; i++)
                foreach (var item in skinnedMaterials)
                {
                    item.SetFloat("_DisolveProgress", _totalDissolve - counter);
                }
               
                yield return new WaitForSeconds(refreshRate);
            }
        }

        if (VFXGraph != null)
        {
            VFXGraph.Stop();
        }

        OnDissolveCompleted?.Invoke();
    }
}
