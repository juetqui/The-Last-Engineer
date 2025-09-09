using System.Collections;
using System.Reflection;
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

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _source.pitch = 1f;
    }

    void Start()
    {
        PlayerNodeHandler.Instance.OnNodeGrabbed += AddEffects;
    }

    private void AddEffects(bool hasNode, NodeType nodeType)
    {
        StopAllCoroutines();

        bool hasChromaticAberration = _volume.profile.TryGet(out ChromaticAberration chromaticAberration);
        bool hasLensDistortion = _volume.profile.TryGet(out LensDistortion lensDistortion);

        if (!hasNode || nodeType != _requiredNode)
        {
            if (hasChromaticAberration)
                StartCoroutine(RemoveEffect(chromaticAberration, 0.15f));

            if (hasLensDistortion)
                StartCoroutine(RemoveEffect(lensDistortion, 0.125f));

            StartCoroutine(IncrementPitch());
            return;
        }

        if (hasChromaticAberration)
            StartCoroutine(AddEffect(chromaticAberration, _maxCAIntensity, 0.25f));

        if (hasLensDistortion)
            StartCoroutine(AddEffect(lensDistortion, _maxLDIntensity, 0.25f));

        StartCoroutine(ReducePitch());
    }

    private IEnumerator AddEffect<T>(T effect, float targetIntensity, float speed = 0.25f) where T : VolumeComponent
    {
        if (effect == null) yield break;

        var intensityProperty = effect.GetType().GetField("intensity", BindingFlags.Public | BindingFlags.Instance);
        
        if (intensityProperty == null || intensityProperty.FieldType != typeof(ClampedFloatParameter)) yield break;

        var intensity = (ClampedFloatParameter)intensityProperty.GetValue(effect);
        
        while (intensity.value < targetIntensity)
        {
            intensity.value += Time.deltaTime * speed;
            yield return null;
        }
        
        intensity.value = targetIntensity;
    }

    private IEnumerator RemoveEffect<T>(T effect, float speed = 0.125f)
    {
        if (effect == null) yield break;

        var intensityProperty = effect.GetType().GetField("intensity", BindingFlags.Public | BindingFlags.Instance);
        
        if (intensityProperty == null || intensityProperty.FieldType != typeof(ClampedFloatParameter)) yield break;

        var intensity = (ClampedFloatParameter)intensityProperty.GetValue(effect);
        
        while (intensity.value > 0)
        {
            intensity.value -= Time.deltaTime * speed;
            yield return null;
        }
        
        intensity.value = 0f;
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
