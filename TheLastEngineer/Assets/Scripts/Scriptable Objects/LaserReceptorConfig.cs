using UnityEngine;

/// <summary>
/// Tuning of a LaserReceptor. Each receptor references one preset by [SerializeField], so receptors
/// that share timings share an asset (no global default through Resources).
/// </summary>
[CreateAssetMenu(fileName = "LaserReceptorConfig", menuName = "Laser/Laser Receptor Config")]
public class LaserReceptorConfig : ScriptableObject
{
    [Tooltip("Seconds a valid laser needs to charge the receptor from empty to full.")]
    [Min(0.01f)]
    [SerializeField] private float _fillTime = 5f;

    [Tooltip("Seconds the receptor takes to empty once no valid laser hits it.")]
    [Min(0.01f)]
    [SerializeField] private float _unfillTime = 0.5f;

    [Tooltip("Seconds the crystal glow (_GlowStep) takes to tween to full or to empty.")]
    [Min(0f)]
    [SerializeField] private float _glowDuration = 3f;

    [Tooltip("If off, a completed receptor stays completed (and active) when the laser leaves.")]
    [SerializeField] private bool _canBeUnfilled = true;

    public float FillTime => _fillTime;
    public float UnfillTime => _unfillTime;
    public float GlowDuration => _glowDuration;
    public bool CanBeUnfilled => _canBeUnfilled;
}
