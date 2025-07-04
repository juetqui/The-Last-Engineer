using System.Collections;
using UnityEngine;

public class PostProcessController : MonoBehaviour
{
    [SerializeField] private Material _passiveMat;
    [SerializeField] private Material _corruptionMat;

    private Coroutine _currentEffect = null;
    private float _vignetteAmount = 9f;
    private float _speed = 60f;

    private NodeType _requiredNode = NodeType.Corrupted;

    void Start()
    {
        _passiveMat.SetFloat("_VignetteAmount", 30f);
        _corruptionMat.SetFloat("_VignetteAmount", 30f);
        PlayerTDController.Instance.OnNodeGrabed += ActivatePassive;
        PlayerTDController.Instance.OnAbsorbCorruption += ActivateCorruption;
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
