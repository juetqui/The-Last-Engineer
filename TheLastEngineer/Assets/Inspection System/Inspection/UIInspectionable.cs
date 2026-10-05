using UnityEngine;

public class UIInspectionable : MonoBehaviour
{
    [SerializeField] private InspectionType _type = InspectionType.None;
    // Referencia explícita: GetComponentInChildren también encontraría los PS de la Corruption.
    [SerializeField] private ParticleSystem _shapeParticles = default;
    [SerializeField] private CleaningProgressShader _cleaningProgress = default;

    // Densidad del objeto sin limpiar; se escala según las corrupciones restantes.
    private int _baseMaxParticles = 0;

    public InspectionType Type {  get { return _type; } }
    public CorruptionGenerator CorruptionGenerator { get; private set; }
    public Corruption UICorruption { get; private set; }

    private void Awake()
    {
        UICorruption = GetComponentInChildren<Corruption>();

        if (_shapeParticles != null)
            _baseMaxParticles = _shapeParticles.main.maxParticles;
    }

    public void SetUpGenerator(CorruptionGenerator generator)
    {
        CorruptionGenerator = generator;
        UICorruption.EnableCorruptionEvents(false);

        if (generator != null)
        {
            CorruptionGenerator = generator;
            CorruptionGenerator.RefreshCorruptionVisual(UICorruption);
            UICorruption.SetUpGenerator(CorruptionGenerator);
            UICorruption.EnableCorruptionEvents(true);

            // El progreso vive en el generador del mundo: al reabrir un objeto a medio limpiar
            // las partículas tienen que arrancar con la densidad que le corresponde.
            RefreshShapeParticles();

            if (_cleaningProgress != null)
                _cleaningProgress.SetUp(CorruptionGenerator);
        }
    }

    public void RefreshCleaningProgress()
    {
        RefreshShapeParticles();

        if (_cleaningProgress != null)
            _cleaningProgress.SyncWithGenerator();
    }

    public void RefreshShapeParticles()
    {
        if (_shapeParticles == null || CorruptionGenerator == null) return;

        int total = CorruptionGenerator.TotalInstances;
        float remaining = total > 0 ? 1f - CorruptionGenerator.CleanedInstances / (float)total : 0f;

        if (remaining <= 0f)
        {
            _shapeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        // Se escala maxParticles y no el rate: con rate * lifetime por encima del máximo, el sistema
        // queda saturado y bajar solo el rate no cambia la cantidad visible.
        var main = _shapeParticles.main;
        main.maxParticles = Mathf.CeilToInt(_baseMaxParticles * remaining);

        if (!_shapeParticles.isEmitting)
            _shapeParticles.Play();
    }
}
