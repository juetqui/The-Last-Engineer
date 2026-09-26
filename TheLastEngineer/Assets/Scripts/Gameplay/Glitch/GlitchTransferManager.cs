public enum GlitchTransferResult
{
    Transferred,
    NoSource,
    NoTarget,
    SourceEmpty,
    TargetFull,
    Invalid
}

/// <summary>
/// Unico lugar donde se mueve carga de glitch. Nada mas puede crear ni destruir cargas, asi que
/// la suma de niveles del sistema cerrado (nodo en mano + objeto) se mantiene constante.
///
/// Devuelve un resultado en vez de disparar feedback: el rumble y los sonidos los resuelve
/// GlitchTransferHandler, del lado del jugador, para no meter dependencias de input ni de audio
/// aca adentro.
/// </summary>
public static class GlitchTransferManager
{
    /// <summary>Misma validacion que ExecuteTransfer pero sin efectos, para el HUD y la seleccion de objetivo.</summary>
    public static GlitchTransferResult Preview(GlitchComponent source, GlitchComponent target)
    {
        // Comparacion contra null con el operador de UnityEngine.Object: tambien cubre destruidos.
        if (source == null) return GlitchTransferResult.NoSource;
        if (target == null) return GlitchTransferResult.NoTarget;
        if (ReferenceEquals(source, target)) return GlitchTransferResult.Invalid;

        if (!source.CanGive()) return GlitchTransferResult.SourceEmpty;
        if (!target.CanReceive()) return GlitchTransferResult.TargetFull;

        return GlitchTransferResult.Transferred;
    }

    /// <summary>
    /// Cuantas cargas se pueden mover de una sola vez: lo que tiene la fuente, limitado por el
    /// espacio libre del destino. 0 si la transferencia no es valida.
    /// </summary>
    public static int MaxTransferable(GlitchComponent source, GlitchComponent target)
    {
        if (Preview(source, target) != GlitchTransferResult.Transferred) return 0;

        return System.Math.Min(source.CurrentLevel, GlitchComponent.MaxLevel - target.CurrentLevel);
    }

    /// <summary>Transferencia de una carga (tap).</summary>
    public static GlitchTransferResult ExecuteTransfer(GlitchComponent source, GlitchComponent target)
        => ExecuteTransfer(source, target, 1);

    /// <summary>
    /// Atomico: valida los dos extremos ANTES de mutar, y si el segundo delta fallara igual
    /// (no deberia) revierte el primero, para que no quede una carga colgada en el limbo.
    /// Mueve las cargas en un solo delta por extremo, asi el hold salta directo al nivel final
    /// (un unico OnGlitchStateChanged) en vez de pasar por el intermedio.
    /// </summary>
    public static GlitchTransferResult ExecuteTransfer(GlitchComponent source, GlitchComponent target, int amount)
    {
        var preview = Preview(source, target);
        if (preview != GlitchTransferResult.Transferred) return preview;

        if (amount < 1 || amount > MaxTransferable(source, target)) return GlitchTransferResult.Invalid;

        if (!source.TryApplyDelta(-amount)) return GlitchTransferResult.Invalid;

        if (!target.TryApplyDelta(amount))
        {
            source.TryApplyDelta(amount);
            return GlitchTransferResult.Invalid;
        }

        return GlitchTransferResult.Transferred;
    }
}
