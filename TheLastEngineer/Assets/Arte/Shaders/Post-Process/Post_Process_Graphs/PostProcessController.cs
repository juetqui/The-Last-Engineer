using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostProcessController : MonoBehaviour
{
    [SerializeField] private UniversalRendererData _rendererData;
    [SerializeField] private Material _passiveMat;
    [SerializeField] private Material _corruptionMat;
    [SerializeField] private Material _shockWaveMat;
    
    [ColorUsage(true, true)]
    [SerializeField] private Color _colorNeg = new Color(191f, 7f, 7f);
    
    [ColorUsage(true, true)]
    [SerializeField] private Color _colorOriginal = new Color(191f, 28f, 164f);

    private ScriptableRendererFeature _shockWave = default;

    private float _vignetteAmount = 9f;
    private float _refNegVignetteAmount = 7f;
    private float _speed = 60f;
    private float duracion = 1f;
    
    private bool animated = false;
    
    private Coroutine _currentEffect = null;

    // THIS SECTION IS USED TO RESTORE THE DEFAULT VALUES OF THE MODIFIED MATERIALS
    #region ORIGINAL VALUES
    private float _origPassiveVignette = default;
    private float _origCorruptionVignette = default;
    private float _origShockwaveDistance = default;

    private Color _origCorruptionColor = Color.black;
    #endregion

    private NodeType _requiredNode = NodeType.Corrupted;

    void Start()
    {
        #region SET ORIGINAL VALUES
        _origPassiveVignette = _passiveMat.GetFloat("_VignetteAmount");
        _origCorruptionVignette = _corruptionMat.GetFloat("_VignetteAmount");
        _origShockwaveDistance = _shockWaveMat.GetFloat("_WaveDistanceFromCenter");
        _origCorruptionColor = _corruptionMat.color;
        #endregion

        _passiveMat.SetFloat("_VignetteAmount", 30f);
        _corruptionMat.SetFloat("_VignetteAmount", 30f);

        PlayerNodeHandler.Instance.OnNodeGrabbed += ActivatePassive;
        PlayerNodeHandler.Instance.OnAbsorbCorruption += ActivateCorruption;

        _shockWave = _rendererData.rendererFeatures.FirstOrDefault(rf => rf is FullScreenPassRendererFeature);
        _shockWave.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_passiveMat != null)
            _passiveMat.SetFloat("_VignetteAmount", _origPassiveVignette);

        if (_corruptionMat != null)
        {
            _corruptionMat.SetFloat("_VignetteAmount", _origCorruptionVignette);
            _corruptionMat.color = _origCorruptionColor;
        }

        if (_shockWaveMat != null)
        {
            _shockWaveMat.SetFloat("_WaveDistanceFromCenter", _origShockwaveDistance);
        }

        if (PlayerNodeHandler.Instance == null) return;

        PlayerNodeHandler.Instance.OnNodeGrabbed -= ActivatePassive;
        PlayerNodeHandler.Instance.OnAbsorbCorruption -= ActivateCorruption;
        PlayerNodeHandler.Instance.OnGlitchChange -= RefNegVignette;
    }

    private void ActivatePassive(bool hasNode, NodeType type)
    {
        if (!hasNode || type != _requiredNode)
        {
            DeactivatePP(_passiveMat);
            PlayerNodeHandler.Instance.OnGlitchChange -= RefNegVignette;
            return;
        }

        ActivatePP(_passiveMat);
        ActivateShockWave();
        PlayerNodeHandler.Instance.OnGlitchChange += RefNegVignette;
    }

    private void ActivateCorruption(bool hasEffect)
    {
        if (!hasEffect)
        {
            DeactivatePP(_corruptionMat);
            return;
        }

        ActivatePP(_corruptionMat);
    }

    private void ActivateShockWave()
    {
        _shockWave.SetActive(true);

        TweenFloat(_shockWaveMat, "_WaveDistanceFromCenter", -0.1f, 1f, 0.5f)
            .setOnComplete(() => _shockWave.SetActive(false));
    }

    private void DeactivateShockWave()
    {
        _shockWave.SetActive(true);

        TweenFloat(_shockWaveMat, "_WaveDistanceFromCenter", 1f, -0.1f, 0.5f)
            .setOnComplete(() => _shockWave.SetActive(false));
    }

    private void ActivatePP(Material mat)
    {
        float start = mat.GetFloat("_VignetteAmount");
        TweenFloat(mat, "_VignetteAmount", start, _vignetteAmount, 0.4f);
    }

    private void DeactivatePP(Material mat)
    {
        float start = mat.GetFloat("_VignetteAmount");
        TweenFloat(mat, "_VignetteAmount", start, _speed / 2f, 0.4f);
    }

    private void RefNegVignette(Glitcheable glt)
    {
        if (animated) return;
        animated = true;

        float current = _passiveMat.GetFloat("_VignetteAmount");

        TweenColor(_passiveMat, "_Color", _colorOriginal, _colorNeg, 0.4f);
        TweenFloat(_passiveMat, "_VignetteAmount", current, _refNegVignetteAmount, 0.4f)
            .setOnComplete(() =>
            {
                TweenColor(_passiveMat, "_Color", _colorNeg, _colorOriginal, 0.4f);
                TweenFloat(_passiveMat, "_VignetteAmount", _refNegVignetteAmount, _vignetteAmount, 0.4f)
                    .setOnComplete(() => animated = false);
            });
    }

    private LTDescr TweenFloat(Material mat, string prop, float from, float to, float dur)
    {
        mat.SetFloat(prop, from);
        return LeanTween.value(gameObject, from, to, dur)
            .setOnUpdate(v => mat.SetFloat(prop, v));
    }

    private LTDescr TweenColor(Material mat, string prop, Color from, Color to, float dur)
    {
        mat.SetColor(prop, from);

        return LeanTween.value(gameObject, 0f, 1f, dur)
            .setOnUpdate(t => mat.SetColor(prop, Color.Lerp(from, to, t)));
    }
}
