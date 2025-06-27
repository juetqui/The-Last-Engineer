using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private float _maxIntensity = 0.15f;
    private NodeType _requiredNode = NodeType.Corrupted;
    private Volume _volume = null;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _source.pitch = 1f;
    }

    void Start()
    {
        PlayerTDController.Instance.OnNodeGrabed += AddCA;
    }

    private void AddCA(bool hasNode, NodeType nodeType)
    {
        if (!hasNode || nodeType != _requiredNode)
        {
            StartCoroutine(RemoveEffect());
            StartCoroutine(IncrementPitch());
            return;
        }

        StartCoroutine(AddEffect());
        StartCoroutine(ReducePitch());
    }

    private IEnumerator AddEffect()
    {
        if (_volume.profile.TryGet(out ChromaticAberration chromaticAberration))
        {
            while (chromaticAberration.intensity.value < _maxIntensity)
            {
                chromaticAberration.intensity.value += Time.deltaTime * 0.25f;
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

    private IEnumerator ReducePitch()
    {
        while (_source.pitch > 0.8f)
        {
            _source.pitch -= Time.deltaTime;
            yield return null;
        }

        _source.pitch = 0.8f;
    }

    private IEnumerator IncrementPitch()
    {
        while (_source.pitch < 1f)
        {
            _source.pitch += Time.deltaTime;
            yield return null;
        }

        _source.pitch = 1f;
    }
}
