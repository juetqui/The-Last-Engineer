using UnityEngine;

/// <summary>
/// Parámetros compartidos por todos los láseres. Antes la máscara del raycast estaba serializada en
/// cada LaserController y los prefabs habían quedado con valores distintos (algunos con Intangible,
/// otros sin).
///
/// Se lee desde Resources (Default) y no por referencia serializada, igual que GlitchPalette: así
/// ningún prefab ni escena necesita cablearla y todos los láseres chocan contra lo mismo.
/// </summary>
[CreateAssetMenu]
public class LaserData : ScriptableObject
{
    private const string ResourcePath = "LaserData";

    // Walls + Laser Collition + Glitched + Intangible: el valor que deberían tener todos los prefabs.
    [Tooltip("Layers contra las que choca el raycast del láser.")]
    public LayerMask layer = (1 << 6) | (1 << 8) | (1 << 18) | (1 << 20);

    private static LaserData _default;

    public static LaserData Default
    {
        get
        {
            if (_default != null) return _default;

            _default = Resources.Load<LaserData>(ResourcePath);

            if (_default == null)
            {
                Debug.LogWarning($"[LaserData] No se encontro Resources/{ResourcePath}.asset: se usan los valores por defecto.");
                _default = CreateInstance<LaserData>();
            }

            return _default;
        }
    }
}
