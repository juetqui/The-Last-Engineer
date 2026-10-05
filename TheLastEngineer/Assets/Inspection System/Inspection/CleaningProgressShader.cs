using UnityEngine;

// Muestrea la gradiente de limpieza de S_Inspectionable vía _CleanProgress (0..1).
// Cada corrupción aporta 1/Total: mientras se mantiene el input se suma ese paso proporcional al hold,
// al completarlo pasa a ser la nueva base y al soltar se vuelve a la base a la misma velocidad.
public class CleaningProgressShader : MonoBehaviour
{
    [SerializeField] private Renderer[] _renderers = default;
    [SerializeField] private string _propertyName = "_CleanProgress";

    private CorruptionGenerator _generator = default;
    private MaterialPropertyBlock _mpb = default;
    private int _propertyId = 0;

    private float _step = 1f;
    private float _base = 0f;
    private float _current = 0f;
    private bool _isHolding = false;
    private bool _listening = false;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _propertyId = Shader.PropertyToID(_propertyName);
    }

    private void Update()
    {
        if (_isHolding || _current <= _base) return;

        // Velocidad simétrica a la de llenado (paso / holdTimer): soltar a la mitad tarda medio hold en volver.
        _current = Mathf.MoveTowards(_current, _base, _step / GetHoldTimer() * Time.unscaledDeltaTime);
        Apply();
    }

    private void OnDisable()
    {
        if (!_listening || CorruptionRemover.Instance == null) return;

        // Solo escucha el objeto activo en el canvas.
        CorruptionRemover.Instance.OnCorruptionHit -= HandleHit;
        CorruptionRemover.Instance.OnHittingCorruption -= HandleHolding;
        _listening = false;
    }

    // Se suscribe acá y no en OnEnable: los objetos UI arrancan activos, antes del Awake de CorruptionRemover.
    public void SetUp(CorruptionGenerator generator)
    {
        _generator = generator;
        int total = _generator != null ? _generator.TotalInstances : 0;
        _step = total > 0 ? 1f / total : 1f;

        SyncWithGenerator();

        if (_listening || CorruptionRemover.Instance == null) return;

        CorruptionRemover.Instance.OnCorruptionHit += HandleHit;
        CorruptionRemover.Instance.OnHittingCorruption += HandleHolding;
        _listening = true;
    }

    // Fija la base según lo ya limpiado; corre al activar el objeto y al completar cada corrupción.
    public void SyncWithGenerator()
    {
        if (_generator == null) return;

        _base = Mathf.Min(1f, _generator.CleanedInstances * _step);
        _current = _base;
        _isHolding = false;
        Apply();
    }

    private void HandleHit(Corruption corruption)
    {
        if (corruption == null)
        {
            // Al soltar, Update baja hasta la base.
            _isHolding = false;
            return;
        }

        // Re-presionar reinicia desde la base: el timer de CorruptionRemover también arranca en 0.
        _isHolding = true;
        _current = _base;
        Apply();
    }

    private void HandleHolding(float timer)
    {
        if (!_isHolding) return;

        _current = Mathf.Min(1f, _base + _step * Mathf.Clamp01(timer / GetHoldTimer()));
        Apply();
    }

    private float GetHoldTimer()
    {
        float holdTimer = CorruptionRemover.Instance != null ? CorruptionRemover.Instance.HoldTimer : 1f;
        return Mathf.Max(holdTimer, 0.0001f);
    }

    // Material compartido (M_Inspection) entre Panel y Battery: se escribe por renderer con MPB.
    // Get antes de Set para no pisar el _SteppedTime que escribe StutterTimePerRenderer en el mismo bloque.
    private void Apply()
    {
        if (_renderers == null) return;

        foreach (var rend in _renderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_propertyId, _current);
            rend.SetPropertyBlock(_mpb);
        }
    }
}
