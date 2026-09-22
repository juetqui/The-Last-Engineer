using System;
using UnityEngine;

/// <summary>
/// Unico contenedor de la verdad sobre el nivel de glitch de un objeto (un NodeController o un
/// Glitcheable). Los comportamientos reactivos (visuales, colliders, FSM) se suscriben a
/// OnGlitchStateChanged en vez de que alguien se los invoque a mano.
/// </summary>
[DisallowMultipleComponent]
public class GlitchComponent : MonoBehaviour
{
    public const int MaxLevel = 2;

    // El nivel ES el campo serializado, sin paso de inicializacion: queda valido desde la
    // deserializacion. Es imprescindible porque Glitcheable corre con DefaultExecutionOrder(-1)
    // y leeria en 0 cualquier init hecha en un Awake de orden normal (incluida la de este mismo
    // componente, que vive en el mismo GameObject pero corre despues).
    [SerializeField] private GlitchState _currentLevel = GlitchState.Clean;

    public GlitchState CurrentState => _currentLevel;
    public int CurrentLevel => (int)_currentLevel;

    public bool CanGive() => CurrentLevel > 0;
    public bool CanReceive() => CurrentLevel < MaxLevel;

    /// <summary>
    /// Solo se emite en los cambios, NUNCA en Awake/Start: emitir el estado inicial seria una
    /// carrera con el orden de inicializacion de los suscriptores. El estado inicial se lee por
    /// pull (CurrentState) desde el Start de cada consumidor.
    /// </summary>
    public event Action<GlitchState> OnGlitchStateChanged;

    /// <summary>
    /// Unico mutador del nivel en todo el proyecto, y solo lo llama GlitchTransferManager.
    /// Rechaza (en vez de clampear) todo lo que se salga de [0, MaxLevel]: el clamp silencioso
    /// destruiria cargas y romperia la conservacion del total nodo + objeto.
    /// </summary>
    /// <summary>
    /// Red de seguridad para un prefab al que se le olvido agregar el componente: lo agrega en
    /// runtime arrancando en Clean y avisa. No reemplaza al componente autorizado en el prefab,
    /// que es el unico lugar donde se puede elegir el nivel inicial.
    /// </summary>
    public static GlitchComponent Ensure(GameObject go)
    {
        var glitch = go.GetComponent<GlitchComponent>();
        if (glitch != null) return glitch;

        Debug.LogWarning($"[GlitchComponent] {go.name} no tiene GlitchComponent: se agrega en runtime " +
                         "arrancando en Clean. Agregalo al prefab para poder setear el nivel inicial.", go);

        return go.AddComponent<GlitchComponent>();
    }

    public bool TryApplyDelta(int delta)
    {
        if (delta == 0) return false;

        var next = CurrentLevel + delta;
        if (next < 0 || next > MaxLevel) return false;

        _currentLevel = (GlitchState)next;
        OnGlitchStateChanged?.Invoke(_currentLevel);

        return true;
    }
}
