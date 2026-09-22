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
    /// Atomico: valida los dos extremos ANTES de mutar, y si el segundo delta fallara igual
    /// (no deberia) revierte el primero, para que no quede una carga colgada en el limbo.
    /// </summary>
    public static GlitchTransferResult ExecuteTransfer(GlitchComponent source, GlitchComponent target)
    {
        var preview = Preview(source, target);
        if (preview != GlitchTransferResult.Transferred) return preview;

        if (!source.TryApplyDelta(-1)) return GlitchTransferResult.Invalid;

        if (!target.TryApplyDelta(1))
        {
            source.TryApplyDelta(1);
            return GlitchTransferResult.Invalid;
        }

        return GlitchTransferResult.Transferred;
    }
}
