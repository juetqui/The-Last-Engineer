using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private float _maxCAIntensity = 0.15f;
    [SerializeField] private float _maxLDIntensity = 0.1f;
    [SerializeField] private float _minPitch = 0.8f;
    [SerializeField] private float _maxPitch = 1f;

    private NodeType _requiredNode = NodeType.Corrupted;
    private Volume _volume = null;

    private ChromaticAberration _chromatic;
    private LensDistortion _lens;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _source.pitch = 1f;

        _volume.profile.TryGet(out _chromatic);
        _volume.profile.TryGet(out _lens);
    }

    void Start()
    {
        PlayerNodeHandler.Instance.OnNodeGrabbed += AddEffects;
    }

    private void AddEffects(bool hasNode, NodeType nodeType)
    {
        StopAllCoroutines();

        bool enable = hasNode && nodeType == _requiredNode;

        if (_chromatic != null)
        {
            if (enable)
                StartCoroutine(AddChromatic(_chromatic, _maxCAIntensity, 0.25f));
            else
                StartCoroutine(RemoveChromatic(_chromatic, 0.15f));
        }

        if (_lens != null)
        {
            if (enable)
                StartCoroutine(AddLens(_lens, _maxLDIntensity, 0.25f));
            else
                StartCoroutine(RemoveLens(_lens, 0.125f));
        }

        if (enable)
            StartCoroutine(ReducePitch());
        else
            StartCoroutine(IncrementPitch());
    }

    private IEnumerator AddChromatic(ChromaticAberration ca, float target, float speed)
    {
        while (ca.intensity.value < target)
        {
            ca.intensity.value += Time.deltaTime * speed;
            yield return null;
        }

        ca.intensity.value = target;
    }

    private IEnumerator RemoveChromatic(ChromaticAberration ca, float speed)
    {
        while (ca.intensity.value > 0f)
        {
            ca.intensity.value -= Time.deltaTime * speed;
            yield return null;
        }

        ca.intensity.value = 0f;
    }

    private IEnumerator AddLens(LensDistortion ld, float target, float speed)
    {
        while (ld.intensity.value < target)
        {
            ld.intensity.value += Time.deltaTime * speed;
            yield return null;
        }

        ld.intensity.value = target;
    }

    private IEnumerator RemoveLens(LensDistortion ld, float speed)
    {
        while (ld.intensity.value > 0f)
        {
            ld.intensity.value -= Time.deltaTime * speed;
            yield return null;
        }

        ld.intensity.value = 0f;
    }

    private IEnumerator ReducePitch()
    {
        while (_source.pitch > _minPitch)
        {
            _source.pitch -= Time.deltaTime;
            yield return null;
        }

        _source.pitch = _minPitch;
    }

    private IEnumerator IncrementPitch()
    {
        while (_source.pitch < _maxPitch)
        {
            _source.pitch += Time.deltaTime;
            yield return null;
        }

        _source.pitch = _maxPitch;
    }
}
