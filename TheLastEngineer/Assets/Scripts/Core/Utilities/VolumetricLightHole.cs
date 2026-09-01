using UnityEngine;

/// <summary>
/// Abre un hueco oscuro en el haz de luz volumetrica (S_VolumetricLights) alrededor del
/// jugador mientras esta dentro del trigger de esta luz.
///
/// La instancia del material se toma recien en OnTriggerEnter y se suelta en OnTriggerExit,
/// restaurando antes los valores originales. Mientras no haya nadie adentro la referencia
/// queda en null y Update no hace ningun calculo.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class VolumetricLightHole : MonoBehaviour
{
    [Header("Hueco")]
    [SerializeField] private float _radius = 3.5f;
    [SerializeField] private float _falloff = 0.1f;
    [Tooltip("0 = cilindro vertical (ignora la altura), 1 = esfera.")]
    [SerializeField, Range(0f, 1f)] private float _yScale = 0f;
    [Tooltip("Cuanto deforma el gradient noise el borde. 0 = circulo perfecto.")]
    [SerializeField, Range(0f, 1f)] private float _noiseStrength = 0.35f;

    [Header("Fade")]
    [SerializeField] private float _fadeSpeed = 4f;

    private static readonly int CenterProp = Shader.PropertyToID("_HoleCenter");
    private static readonly int RadiusProp = Shader.PropertyToID("_HoleRadius");
    private static readonly int FalloffProp = Shader.PropertyToID("_HoleFalloff");
    private static readonly int StrengthProp = Shader.PropertyToID("_HoleStrength");
    private static readonly int YScaleProp = Shader.PropertyToID("_HoleYScale");
    private static readonly int NoiseStrengthProp = Shader.PropertyToID("_HoleNoiseStrength");

    private Renderer _renderer;
    private Collider _trigger;
    private Material _material;   // solo tiene valor mientras el jugador esta adentro
    private Transform _target;
    private float _strength;

    // Valores originales del material, para devolverlo como estaba al salir.
    private Vector4 _originalCenter;
    private float _originalRadius;
    private float _originalFalloff;
    private float _originalYScale;
    private float _originalNoiseStrength;
    private float _originalStrength;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _trigger = GetComponent<Collider>();

        // sharedMaterial: leemos los defaults sin instanciar nada todavia.
        Material shared = _renderer.sharedMaterial;
        _originalCenter = shared.GetVector(CenterProp);
        _originalRadius = shared.GetFloat(RadiusProp);
        _originalFalloff = shared.GetFloat(FalloffProp);
        _originalYScale = shared.GetFloat(YScaleProp);
        _originalNoiseStrength = shared.GetFloat(NoiseStrengthProp);
        _originalStrength = shared.GetFloat(StrengthProp);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_material != null || !IsPlayer(other)) return;

        _target = PlayerController.Instance != null ? PlayerController.Instance.transform : other.transform;
        _material = _renderer.material;   // instancia propia de este haz
        _strength = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (_material == null || !IsPlayer(other)) return;
        Release();
    }

    private void OnDisable() => Release();

    private void Update()
    {
        if (_material == null) return;

        // PlayerInteractionDetector documenta que en este proyecto no se puede confiar del todo
        // en OnTriggerExit (respawns, teleports). Revalidamos para que el hueco no quede pegado.
        if (!IsStillInside())
        {
            Release();
            return;
        }

        _strength = Mathf.MoveTowards(_strength, 1f, _fadeSpeed * Time.deltaTime);

        _material.SetVector(CenterProp, _target.position);
        _material.SetFloat(RadiusProp, Mathf.Max(_radius, 0.001f)); // el shader divide por este valor
        _material.SetFloat(FalloffProp, Mathf.Max(_falloff, 0.01f));
        _material.SetFloat(YScaleProp, _yScale);
        _material.SetFloat(NoiseStrengthProp, _noiseStrength);
        _material.SetFloat(StrengthProp, _strength);
    }

    /// <summary>Devuelve el material a sus valores originales y suelta la referencia.</summary>
    private void Release()
    {
        if (_material == null) return;

        _material.SetVector(CenterProp, _originalCenter);
        _material.SetFloat(RadiusProp, _originalRadius);
        _material.SetFloat(FalloffProp, _originalFalloff);
        _material.SetFloat(YScaleProp, _originalYScale);
        _material.SetFloat(NoiseStrengthProp, _originalNoiseStrength);
        _material.SetFloat(StrengthProp, _originalStrength);

        _material = null;
        _target = null;
        _strength = 0f;
    }

    private bool IsStillInside()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy) return false;
        if (_trigger == null) return true;

        // Bounds del trigger con un margen: solo queremos descartar al jugador cuando esta
        // claramente afuera, no pelear con el borde exacto del collider.
        Bounds bounds = _trigger.bounds;
        bounds.Expand(_radius);
        return bounds.Contains(_target.position);
    }

    private static bool IsPlayer(Collider other) => other.GetComponentInParent<PlayerController>() != null;
}
