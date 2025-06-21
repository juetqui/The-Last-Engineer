using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField] private NodeType _requiredNode = NodeType.Purple;
    [SerializeField] private float _maxIntensity = 0.15f;
    private Volume _volume = null;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += AddCA;
    }

    private void AddCA(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType != _requiredNode)
        {
            Debug.Log("Remove");
            StartCoroutine(RemoveEffect());
            return;
        }

        Debug.Log("Add");
        StartCoroutine(AddEffect());
    }

    private IEnumerator AddEffect()
    {
        if (_volume.profile.TryGet(out ChromaticAberration chromaticAberration))
        {
            while (chromaticAberration.intensity.value < _maxIntensity)
            {
                chromaticAberration.intensity.value += Time.deltaTime * 0.125f;
                yield return null;
            }

            chromaticAberration.intensity.value = _maxIntensity;
        }
    }

    private IEnumerator RemoveEffect()
    {
        if (_volume.profile.TryGet(out ChromaticAberration chromaticAberration))
        {
            while (chromaticAberration.intensity.value > 0f)
            {
                chromaticAberration.intensity.value -= Time.deltaTime * 0.125f;
                yield return null;
            }

            chromaticAberration.intensity.value = Mathf.Floor(chromaticAberration.intensity.value);
        }
    }
}
