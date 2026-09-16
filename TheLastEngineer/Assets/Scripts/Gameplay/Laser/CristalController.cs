using UnityEngine;

public class CristalController : MonoBehaviour
{
    private Glitcheable _glitcheable;
    private Renderer _cristalShader;
    private LaserActivator _activator;

    private void Awake()
    {
        _glitcheable = GetComponentInParent<Glitcheable>();
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
        if (_glitcheable != null)
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
