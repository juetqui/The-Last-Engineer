using System.Collections;
using UnityEngine;

public class GlitchController : MonoBehaviour
{
    [SerializeField] private float _minCD;
    [SerializeField] private float _maxCD;

    [SerializeField] private float _minDist;
    [SerializeField] private float _maxDist;

    private Material _material;
    private bool _enabled = true;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (_enabled)
        {
            StartCoroutine(ApplyGlitchEffect());
        }
    }

    private IEnumerator ApplyGlitchEffect()
    {
        float glitchCD = Random.Range(_minCD, _maxCD);
        float glitchAmount = Random.Range(_minDist, _maxDist);
        float secGlitchAmount = Random.Range(_minDist, _maxDist);
        float stepAmount = Random.Range(0.45f, 0.6f);

        _enabled = false;
        _material.SetFloat("_GlitchStrength", glitchAmount);
        _material.SetFloat("_GlitchStrength_1", secGlitchAmount);
        _material.SetFloat("_StepAmount", stepAmount);

        yield return new WaitForSeconds(glitchCD);

        _material.SetFloat("_GlitchStrength", 0f);
        _material.SetFloat("_GlitchStrength_1", 0f);
        _material.SetFloat("_StepAmount", 1f);

        yield return new WaitForSeconds(glitchCD);

        _enabled = true;
    }
}
