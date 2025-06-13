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
    public Shader MyShader;
    public float refreshRate;
    [SerializeField] float duration;
    [SerializeField] Vector3 killerSize;
    [SerializeField] Vector3 particleSpeed;
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
        _totalDissolve = skinnedMaterials[0].GetFloat("_DisolveProgress");

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
        StartCoroutine(DissolveCo());
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
                item.shader = MyShader;
                item.SetFloat("_MaxHeight", MaxBound);
                item.SetFloat("_MinHeight", MinBound);
            }
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
}
