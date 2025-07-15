using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GlitchDeathController : MonoBehaviour
{
    public static GlitchDeathController Instance;
    
    [Header("Material del Shader Graph")]
    public Material glitchMaterial;

    [Header("Duración total del efecto (en segundos)")]
    public float glitchDuration = 2f;

    private bool isGlitching = false;
    private float timer = 0f;

    void Awake()
    {
        glitchMaterial.SetFloat("_FullScreenColor", 0f);
        glitchMaterial.SetFloat("_NoGlitchInScreen", 0f);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Por si lo instanciás más de una vez sin querer
        }
    }

    void Update()
    {
        if (!isGlitching) return;

        timer += Time.deltaTime;
        float t = timer / glitchDuration;

        if (t <= 0.4f)
        {
            float localT = t / 0.2f;
            glitchMaterial.SetFloat("_FullScreenColor", 0f);
            glitchMaterial.SetFloat("_NoGlitchInScreen", Mathf.Lerp(0f, 0.5f, localT));
        }
        else if (t <= 0.6f)
        {
            float localT = (t - 0.2f) / 0.4f;
            glitchMaterial.SetFloat("_FullScreenColor", Mathf.Lerp(0.1f, 1f, localT));
            glitchMaterial.SetFloat("_NoGlitchInScreen", 0.5f);
        }
        else if (t <= 0.8f)
        {
            float localT = (t - 0.6f) / 0.2f;
            glitchMaterial.SetFloat("_FullScreenColor", Mathf.Lerp(1f, 0.5f, localT));
            glitchMaterial.SetFloat("_NoGlitchInScreen", 1f);
        }
        else if (t <= 1f)
        {
            float localT = (t - 0.8f) / 0.2f;
            float val = Mathf.Lerp(0.5f, 0f, localT);
            glitchMaterial.SetFloat("_FullScreenColor", val);
            glitchMaterial.SetFloat("_NoGlitchInScreen", val);
        }

        if (t >= 1f)
        {
            glitchMaterial.SetFloat("_FullScreenColor", 0f);
            glitchMaterial.SetFloat("_NoGlitchInScreen", 0f);
            isGlitching = false;
        }
    }

    public void TriggerGlitch()
    {
        timer = 0f;
        isGlitching = true;
    }
}



