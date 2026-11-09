using UnityEngine;

public class CristalController : MonoBehaviour
{
    // Glitcheable and Intangible lasers both show the glitch effect: the player can swap their state.
    private bool _isSwapTarget;
    private Renderer _cristalShader;
    private LaserActivator _activator;

    private void Awake()
    {
        _isSwapTarget = GetComponentInParent<IGlitchSwapTarget>() != null;
        _cristalShader = GetComponent<Renderer>();
    }

    private void Start()
    {
        _activator = GetComponentInParent<LaserActivator>();
        SetGlitcheableCristalView();
        SetBrightnes();
    }

    public void SetGlitcheableCristalView()
    {
        if (_isSwapTarget)
        {
            _cristalShader.material.SetFloat("_GlitchAmount", 1);
        }
        else
        {
            _cristalShader.material.SetFloat("_GlitchAmount", 0);
        }
    }

    public void SetBrightnes()
    {
        if(_activator.LaserStartsInitialized)
        {
            _cristalShader.material.SetFloat("_IsInitialized", 1);
        }
        else
        {
            _cristalShader.material.SetFloat("_IsInitialized", 0);
        }
    }
}
