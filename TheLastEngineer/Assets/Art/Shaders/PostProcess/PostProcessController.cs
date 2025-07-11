using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines.Interpolators;

public class PostProcessController : MonoBehaviour
{
    [SerializeField] private Material _passiveMat;
    [SerializeField] private Material _corruptionMat;
    [ColorUsage(true, true)]
    [SerializeField] private Color _colorNeg = new Color(191f, 7f, 7f);
    [ColorUsage(true, true)]
    [SerializeField] private Color _colorOriginal = new Color(191f, 28f, 164f);
    
    private float duracion = 1f;
    private Coroutine _currentEffect = null;
    private float _vignetteAmount = 9f;
    private float _refNegVignetteAmount = 7f;
    private float _speed = 60f;
    private bool animated = false;

    private NodeType _requiredNode = NodeType.Corrupted;

    void Start()
    {
        _passiveMat.SetFloat("_VignetteAmount", 30f);
        _corruptionMat.SetFloat("_VignetteAmount", 30f);
        PlayerTDController.Instance.OnNodeGrabed += ActivatePassive;
        PlayerTDController.Instance.OnAbsorbCorruption += ActivateCorruption;
        GlitchActive.Instance.OnChangeObjectState += RefNegVignette;
    }

    private void ActivatePassive(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType != _requiredNode)
        {
            StartCoroutine(DeactivatePP(_passiveMat));
            return;
        }

        StartCoroutine(ActivatePP(_passiveMat));
    }

    private void ActivateCorruption(bool hasEffect)
    {
        if (_currentEffect != null)
            StopCoroutine(_currentEffect);

        if (!hasEffect)
        {
            _currentEffect = StartCoroutine(DeactivatePP(_corruptionMat));
            return;
        }

        _currentEffect = StartCoroutine(ActivatePP(_corruptionMat));
    }

    private IEnumerator ActivatePP(Material material)
    {
        float currentAmount = material.GetFloat("_VignetteAmount");
        while (currentAmount > _vignetteAmount)
        {
            currentAmount -= Time.deltaTime * _speed;
            material.SetFloat("_VignetteAmount", currentAmount);

            yield return null;
        }

        material.SetFloat("_VignetteAmount", _vignetteAmount);
    }

    private void RefNegVignette(Glitcheable gltich, InteractionOutcome interact)
    {
        if(animated == false)
        {
            if (interact.Result == InteractResult.Invalid)
            {
                StartCoroutine(LerpColorRefNeg(_passiveMat));
            }
        }
    }

    private IEnumerator LerpColorRefNeg(Material material)
    {
        animated = true;

        float currentAmount = material.GetFloat("_VignetteAmount");
        while (currentAmount > _refNegVignetteAmount)
        {
            _passiveMat.color = Color.Lerp(_colorOriginal, _colorNeg, duracion);
            currentAmount -= Time.deltaTime * 10f;
            material.SetFloat("_VignetteAmount", currentAmount);

            yield return null;
        }

        while (currentAmount < _vignetteAmount)
        {
            _passiveMat.color = Color.Lerp(_colorNeg, _colorOriginal, duracion);
            currentAmount += Time.deltaTime * 15f;
            material.SetFloat("_VignetteAmount", currentAmount);

            yield return null;
        }
        material.SetFloat("_VignetteAmount", _vignetteAmount);
        
        yield return new WaitForSeconds(1f);
        animated = false;
    }

    private IEnumerator DeactivatePP(Material material)
    {
        float speed = (_speed / 2f);
        float currentAmount = material.GetFloat("_VignetteAmount");

        while (currentAmount < speed)
        {
            currentAmount += Time.deltaTime * speed;
            material.SetFloat("_VignetteAmount", currentAmount);

            yield return null;
        }

        material.SetFloat("_VignetteAmount", speed);
    }
}
